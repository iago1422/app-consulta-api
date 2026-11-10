using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Spark.Api.Services;
using Spark.Domain.Commands.Usuario;
using Spark.Domain.Configuracao;
using Spark.Domain.Handlers;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;

namespace Spark.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = Perfis.Equipe)]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguracaoRepository _config;

        public AdminController(DataContext context, IConfiguracaoRepository config)
        {
            _context = context;
            _config = config;
        }

        // PUT /admin/config/consultas
        [HttpPut("config/consultas")]
        public async Task<IActionResult> SalvarConfig([FromBody] ConfigConsultas config)
        {
            if (config == null)
                return BadRequest(new { message = "Configuração vazia." });

            var erros = config.Validar();
            if (erros.Count > 0)
                return BadRequest(new { message = string.Join(" ", erros), erros });

            await _config.SalvarConsultas(config, User.UsuarioId());
            return Ok(await ConfigConsultasResposta.Montar(_config, _context));
        }

        private static DateTime? Utc(DateTime? d) => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null;

        // GET /admin/atendimentos?de=&ate=&medicoId=&status=&pagina=1&tamanho=50
        [HttpGet("atendimentos")]
        public async Task<IActionResult> Atendimentos([FromQuery] DateTime? de, [FromQuery] DateTime? ate, [FromQuery] Guid? medicoId,
            [FromQuery] string status, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 50)
        {
            var fim = (ate ?? DateTime.UtcNow).ToUniversalTime();
            var inicio = (de ?? fim.AddDays(-30)).ToUniversalTime();
            tamanho = Math.Clamp(tamanho, 1, 200);
            pagina = Math.Max(pagina, 1);

            var query = FiltroAtendimentos.Aplicar(_context.FilaAtendimento.AsNoTracking(), inicio, fim, medicoId, status);
            var total = await query.CountAsync();
            var filas = await query.Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync();

            var fichaIds = filas.Select(f => f.FichaId).Distinct().ToList();
            var fichas = await _context.FichaClinicas.AsNoTracking()
                .Where(f => fichaIds.Contains(f.Id))
                .Select(f => new { f.Id, f.NomeCompleto, f.Telefones })
                .ToListAsync();

            var medicoIds = filas.Where(f => f.MedicoId.HasValue).Select(f => f.MedicoId.Value).Distinct().ToList();
            var medicos = await _context.Usuarios.AsNoTracking()
                .Where(u => medicoIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Nome })
                .ToListAsync();

            var referencias = filas.Select(f => f.Id.ToString()).ToList();
            var cobrancas = await _context.MovimentacoesCredito.AsNoTracking()
                .Where(m => m.Tipo == "USO" && referencias.Contains(m.Referencia))
                .GroupBy(m => m.Referencia)
                .Select(g => new { Referencia = g.Key, Total = g.Sum(m => m.Valor) })
                .ToListAsync();

            var itens = filas.Select(f =>
            {
                var ficha = fichas.FirstOrDefault(x => x.Id == f.FichaId);
                return new
                {
                    id = f.Id,
                    entrouEm = Utc(f.CreatedAt),
                    chamadoEm = Utc(f.CalledAt),
                    finalizadoEm = f.Status == "DONE" || f.Status == "LEFT" ? Utc(f.UpdatedAt) : null,
                    esperaMin = f.CalledAt.HasValue ? (int?)Math.Round((f.CalledAt.Value - f.CreatedAt).TotalMinutes) : null,
                    tipo = f.Tipo,
                    status = FiltroAtendimentos.Status(f),
                    fichaId = f.FichaId,
                    anamneseId = f.AnamnseId,
                    pacienteNome = ficha?.NomeCompleto,
                    pacienteTelefone = ficha?.Telefones?.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)),
                    medicoId = f.MedicoId,
                    medicoNome = medicos.FirstOrDefault(m => m.Id == f.MedicoId)?.Nome,
                    creditosCobrados = cobrancas.FirstOrDefault(c => c.Referencia == f.Id.ToString())?.Total ?? 0
                };
            });

            return Ok(new { total, itens });
        }

        // GET /admin/medicos
        [HttpGet("medicos")]
        public async Task<IActionResult> Medicos()
        {
            // Equipe inteira: há médico em produção no terceiro perfil (e0e938ef-…), não só em MEDICO
            var perfilMedico = Guid.Parse(Perfis.Medico);
            var perfilTerceiro = Guid.Parse(Perfis.Terceiro);
            var medicos = await _context.Usuarios.AsNoTracking()
                .Where(u => u.PerfilId == perfilMedico || u.PerfilId == perfilTerceiro)
                .OrderBy(u => u.Nome)
                .Select(u => new { id = u.Id, nome = u.Nome, email = u.Email })
                .ToListAsync();
            return Ok(new { data = medicos });
        }

        public class NovoMedico
        {
            public string Nome { get; set; }
            public string Email { get; set; }
            public string Senha { get; set; }
            public string Cpf { get; set; }
        }

        // POST /admin/medicos — o cadastro público não aceita mais MEDICO
        [HttpPost("medicos")]
        public async Task<IActionResult> CriarMedico([FromBody] NovoMedico body, [FromServices] UsuarioHandler handler)
        {
            if (body == null)
                return BadRequest(new { message = "Dados do médico são obrigatórios." });

            var r = await handler.Handle(new CriarUsuario.Request
            {
                Nome = body.Nome, Email = body.Email, Senha = body.Senha, Cpf = body.Cpf, Perfil = "MEDICO", Imagem = ""
            });

            // Não devolve r.Data: é o comando, com a senha
            if (!r.Success)
            {
                var detalhes = r.Data is IEnumerable<Flunt.Notifications.Notification> n
                    ? string.Join(" ", n.Select(x => x.Message))
                    : r.Message;
                return BadRequest(new { message = detalhes });
            }
            return Ok(new { message = "Médico cadastrado." });
        }
    }
}
