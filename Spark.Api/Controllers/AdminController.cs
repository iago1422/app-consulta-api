using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spark.Api.Services;
using Spark.Domain.Configuracao;
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
    }
}
