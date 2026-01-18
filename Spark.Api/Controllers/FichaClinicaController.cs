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
    [Authorize(Roles = "acb3830a-402b-4865-9f70-7b28d39f66ad,efe703c2-2076-4d5b-aa04-aaee90953e49,e0e938ef-5ca3-48bc-b5a5-30a0fd0b64ca")]
    [Route("ficha-clinica")]
    public class FichaClinicaController : ControllerBase
    {
        private readonly IFichaClinicaRepository _repository;

        public FichaClinicaController(IFichaClinicaRepository repository)
        {
            _repository = repository;
        }

        private Guid GetUsuarioLogadoId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var userId))
                throw new UnauthorizedAccessException("Token sem identificador do usuário.");

            return userId;
        }

        [Route("get-by-paciente/{pacienteId}")]
        [HttpGet]
        public async Task<IActionResult> GetByPacienteId([FromRoute] Guid pacienteId)
        {
            var ficha = await _repository.GetByPacienteId(pacienteId);

            if (ficha == null)
                return BadRequest("Sem vinculo ou sem ficha."); // sem vínculo (ou sem ficha)

            return Ok(new { Data = ficha });
        }

        /// <summary>
        /// Mantém o uso do Handler como era antes.
        /// A única diferença: agora a controller injeta o usuarioLogadoId no command
        /// (o handler/repository precisa usar isso para validar vínculo).
        /// </summary>
        [Route("")]
        [HttpPost]
        public async Task<GenericCommandResult> Create(
            [FromBody] CriarFichaClinica.Request command,
            [FromServices] FichaClinicaHandler handler)
        {          
           return  await handler.Handle(command);          
        }

        [Route("")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CriarFichaClinica.Request command)
        {
            var usuarioLogadoId = GetUsuarioLogadoId();

            // mesmo esquema do POST
            command.UsuarioLogadoId = usuarioLogadoId;

            var result = await _repository.Update(command);

            if (!result.Sucess)
                return BadRequest(result);

            return Ok(result);
        }
        [Route("")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repository.getAll();
            return Ok(new { Data = result });
        }


        [Route("get-by-id/{fichaId}")]
        [HttpGet]
        public async Task<IActionResult> GetById([FromRoute] Guid fichaId)
        {

            var ficha = await _repository.GetById(fichaId);

            if (ficha == null)
                return BadRequest("Sem vinculo ou sem ficha."); // sem vínculo (ou sem ficha)

            return Ok(new { Data = ficha });
        }

    }
}
