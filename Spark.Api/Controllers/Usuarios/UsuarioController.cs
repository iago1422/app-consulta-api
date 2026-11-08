using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Spark.Domain.Commands;
using Spark.Domain.Commands.Usuario;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Handlers;
using Spark.Domain.Repositories;
using System.Threading.Tasks;

namespace Spark.Api.Controllers.Usuarios
{
    [ApiController]
    [Authorize(Roles = "acb3830a-402b-4865-9f70-7b28d39f66ad,efe703c2-2076-4d5b-aa04-aaee90953e49,e0e938ef-5ca3-48bc-b5a5-30a0fd0b64ca")]
    [Route("usuarios")]
    public class UsuarioController : Controller
    {
        [Route("usuario-unico/{id}")]
        [HttpGet]
        ///get
        public ObterUsuario.Response GetById([FromServices]IUsuarioRepository repository, [FromRoute] Guid id)
        {
            return repository.GetById(id);
        }

        [Route("")]
        [HttpGet]
        ///get
        public List<ObterUsuario.Response> GetAll([FromServices] IUsuarioRepository repository)
        {
            return repository.GetAll();
        }


        [Route("")]
        [AllowAnonymous]
        [HttpPost]
        ///post
        public async Task<GenericCommandResult> Create([FromBody] CriarUsuario.Request command, [FromServices]UsuarioHandler handler)
        {
            return await handler.Handle(command);
        }

        [Route("")]
        [HttpPut]
        ///put
        public async Task<IActionResult> Update([FromBody] AtualizarUsuario.Request command, [FromServices]UsuarioHandler handler)
        {
            if (!EhOProprioUsuario(command.Id))
                return Forbid();

            return Ok(await handler.Handle(command));
        }

        [Route("")]
        [HttpDelete]
        ///post
        public async Task<IActionResult> Delete([FromBody] DeletarUsuario.Request command, [FromServices] UsuarioHandler handler)
        {
            if (!EhOProprioUsuario(command.Id))
                return Forbid();

            return Ok(await handler.Handle(command));
        }

        // Só o próprio usuário pode alterar/excluir a própria conta (id do token = NameIdentifier, ver TokenService)
        private bool EhOProprioUsuario(Guid id) =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioLogadoId) && usuarioLogadoId == id;

    }
}
