using Microsoft.AspNetCore.Mvc;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Spark.Api.Controllers.FaceInsta
{
    [ApiController]    
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