using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Repositories;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Spark.Api.Controllers.FaceInsta
{
    [ApiController]
    [Authorize(Roles = "acb3830a-402b-4865-9f70-7b28d39f66ad,efe703c2-2076-4d5b-aa04-aaee90953e49,e0e938ef-5ca3-48bc-b5a5-30a0fd0b64ca")]
    [Route("perfil")]
    public class PerfilController : ControllerBase
    {
        [Route("perfil-unica/{id}")]
        [HttpGet]
        ///get
        public Perfil GetById([FromServices] IPerfilRepository repository, [FromRoute] Guid id)
        {
            return repository.GetById(id);
        }

        [Route("{skip:int}/{take:int}")]
        [HttpGet]
        ///get
        public async Task<IActionResult> GetAll([FromServices] IPerfilRepository repository, [FromRoute] int skip, [FromRoute] int take )
        {
            return Ok(new { Data = await repository.GetAll(skip, take) });
        }       
    }
}