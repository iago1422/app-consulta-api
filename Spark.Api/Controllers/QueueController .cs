using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Entities;
using Spark.Domain.Commands;

namespace Spark.Api.Controllers
{
    [ApiController]
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
            if (request == null || request.TenantId == Guid.Empty || request.PacienteId == Guid.Empty)
                return BadRequest(new { Message = "TenantId e PacienteId são obrigatórios." });

            var entity = new FilaAtendimento
            {
                Id = Guid.NewGuid(), // ou deixa o banco gerar se você colocou defaultValueSql
                TenantId = request.TenantId,
                PacienteId = request.PacienteId,
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
    }
}
