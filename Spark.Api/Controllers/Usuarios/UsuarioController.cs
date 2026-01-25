using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
        public Usuario GetById([FromServices]IUsuarioRepository repository, [FromRoute] Guid id)
        {
            return repository.GetById(id);
        }

        [Route("")]
        [HttpGet]
        ///get
        public List<Usuario> GetAll([FromServices] IUsuarioRepository repository)
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
        public async Task<GenericCommandResult> Update([FromBody] AtualizarUsuario.Request command, [FromServices]UsuarioHandler handler)
        {
            return await handler.Handle(command);
        }

        [Route("")]
        [HttpDelete]
        ///post
        public async Task<GenericCommandResult> Delete([FromBody] DeletarUsuario.Request command, [FromServices] UsuarioHandler handler)
        {
            return await handler.Handle(command);
        }

    }
}
