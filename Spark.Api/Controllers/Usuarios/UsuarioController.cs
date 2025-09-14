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
    //[Authorize(Roles = "840d02b2-2607-46e3-bbfc-7f454de0aeec")]
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

    }
}
