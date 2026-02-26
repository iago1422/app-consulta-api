using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Entities;
using Spark.Domain.Commands;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Spark.Infra.Migrations;
using static Amazon.S3.Util.S3EventNotification;

namespace Spark.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "acb3830a-402b-4865-9f70-7b28d39f66ad,efe703c2-2076-4d5b-aa04-aaee90953e49,e0e938ef-5ca3-48bc-b5a5-30a0fd0b64ca")]
    [Route("queue")]
    public class QueueController : ControllerBase
    {
        private readonly DataContext _context;

        public QueueController(DataContext context)
        {
            _context = context;
        }

        // =========================
        // PACIENTE
        // =========================

        // POST /queue/join
        [HttpPost("join")]
        public async Task<IActionResult> Join([FromBody] JoinQueueRequest.Request request)
        {
            if (request == null || request.TenantId == Guid.Empty || request.FichaId == Guid.Empty || request.AnamneseId == Guid.Empty)
                return BadRequest(new { Message = "TenantI,  FichaId e AnamneseId são obrigatórios." });

            var entity = new FilaAtendimento
            {
                Id = Guid.NewGuid(), // ou deixa o banco gerar se você colocou defaultValueSql
                TenantId = request.TenantId,
                FichaId = request.FichaId,
                AnamnseId = request.AnamneseId,
                Tipo = request.Tipo,
                Status = "WAITING",
                CreatedAt = DateTime.UtcNow
            };

            _context.FilaAtendimento.Add(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // se você criou o índice parcial uq_fila_paciente_ativo,
                // esse catch cobre o cenário do paciente já estar na fila
                return Conflict(new { Message = "Paciente já está na fila (WAITING/CALLED)." });
            }

            return Ok(new
            {
                queueId = entity.Id,
                status = entity.Status
            });
        }

        // POST /queue/leave
        [HttpPost("leave")]
        public async Task<IActionResult> Leave([FromBody] LeaveQueueRequest.Request request)
        {
            try
            {
                if (request == null || request.TenantId == Guid.Empty || request.FichaId == Guid.Empty || request.AnamneseId == Guid.Empty)
                    return BadRequest(new { Message = "TenantId, FichaId e AnamneseId são obrigatórios." });

                // Busca o registro ativo do paciente na fila
                var entity = await _context.FilaAtendimento
                    .Where(x => x.TenantId == request.TenantId
                             && x.FichaId == request.FichaId
                             && x.AnamnseId == request.AnamneseId
                             && (x.Status == "WAITING" || x.Status == "CALLED"))
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (entity == null)
                    return NotFound(new { Message = "Paciente não está na fila (WAITING/CALLED)." });

                entity.Status = "LEFT";
                entity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Sucess = true,
                    Message = "Você saiu da fila.",
                    Data = new
                    {
                        queueId = entity.Id,
                        status = entity.Status
                    }
                });
            } catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucess = false,
                    Message = "Erro ao tentar sair da fila."
                });
            }
           
        }

        // GET /queue/{id}/position
        [HttpGet("{id:guid}/position")]
        public async Task<IActionResult> Position([FromRoute] Guid id)
        {
            var current = await _context.FilaAtendimento
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (current == null)
                return NotFound(new { Message = "Registro da fila não encontrado." });

            if (current.Status != "WAITING")
                return Ok(new { position = 0, status = current.Status }); // 0 = não está aguardando

            var aheadCount = await _context.FilaAtendimento
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == current.TenantId &&
                    x.Status == "WAITING" &&
                    x.CreatedAt < current.CreatedAt)
                .CountAsync();

            return Ok(new
            {
                position = aheadCount + 1,
                status = current.Status
            });
        }

        // =========================
        // MÉDICO
        // =========================

        [HttpPost("next")]
        public async Task<IActionResult> Next([FromBody] NextQueueRequest.Request request)
        {
            if (request == null || request.TenantId == Guid.Empty)
                return BadRequest(new { Message = "TenantId é obrigatório." });

            using var tx = await _context.Database.BeginTransactionAsync();

            // 1) Buscar a PRÓXIMA ENTIDADE (não só Id) com lock
            var nextEntity = await _context.FilaAtendimento
                .FromSqlRaw(@"
            select *
            from ""FilaAtendimento""
            where ""TenantId"" = {0}
              and ""Status"" = 'WAITING'
            order by ""CreatedAt"" asc
            for update skip locked
            limit 1
        ", request.TenantId)
                .FirstOrDefaultAsync();

            if (nextEntity == null)
            {
                await tx.CommitAsync();
                return NoContent();
            }

            // 2) Atualiza via EF
            nextEntity.Status = "CALLED";
            nextEntity.CalledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(nextEntity);
        }



        // POST /queue/{id}/finish
        [HttpPost("{id:guid}/finish")]
        public async Task<IActionResult> Finish([FromRoute] Guid id)
        {
            var entity = await _context.FilaAtendimento.FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(new { Message = "Registro da fila não encontrado." });

            if (entity.Status != "CALLED")
                return BadRequest(new { Message = "Só é possível finalizar quando o status estiver CALLED." });

            entity.Status = "DONE";
            await _context.SaveChangesAsync();

            return Ok(new { status = "DONE" });
        }


        // GET /queue/list?tenantId=...
        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                return BadRequest(new { Message = "tenantId é obrigatório." });

            var itens = await _context.FilaAtendimento
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Status != "DONE")
                .OrderBy(x => x.Status == "WAITING" ? 0 : x.Status == "CALLED" ? 1 : 2)
                .ThenBy(x => x.CreatedAt)
                .Select(x => new
                {
                    id = x.Id,
                    tenantId = x.TenantId,
                    anamneseId = x.AnamnseId,
                    fichaId = x.FichaId,
                    status = x.Status,
                    createdAt = x.CreatedAt,
                    calledAt = x.CalledAt,
                    tipo = x.Tipo
                })
                .ToListAsync();

            return Ok(new { data = itens });
        }

    }
}
