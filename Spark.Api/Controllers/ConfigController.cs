using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spark.Api.Services;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;

namespace Spark.Api.Controllers
{
    [ApiController]
    [Route("config")]
    public class ConfigController : ControllerBase
    {
        // GET /config/consultas — público: o app mostra os preços e o portal o custo no encerramento
        [HttpGet("consultas")]
        [AllowAnonymous]
        public async Task<IActionResult> Consultas([FromServices] IConfiguracaoRepository repo, [FromServices] DataContext context) =>
            Ok(await ConfigConsultasResposta.Montar(repo, context));
    }
}
