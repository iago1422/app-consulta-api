using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spark.Domain.Commands;
using Spark.Domain.Handlers;
using Spark.Domain.Repositories;

namespace Spark.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "acb3830a-402b-4865-9f70-7b28d39f66ad")]
    [Route("ficha-clinica")]
    public class UsuarioPacienteController : ControllerBase
    {
        private Guid GetUsuarioLogadoId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(id, out var userId))
                throw new UnauthorizedAccessException("Usuário não identificado no token.");

            return userId;
        }

        [HttpPost("vincular")]
        public async Task<IActionResult> VincularPaciente(
            [FromBody] CriarVinculoUsuarioPaciente.Request command,
            [FromServices] UsuarioPacienteHandler handler)
        {
            command.UsuarioLogadoId = GetUsuarioLogadoId();

            var result = await handler.Handle(command);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}