using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spark.Domain.Commands;
using Spark.Domain.Entities;
using Spark.Domain.Handlers;
using Spark.Domain.Handlers.Imagem;
using Spark.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace Spark.Api.Controllers
{
    [ApiController]
    //[Authorize(Roles = "d7ccf34b-b722-4735-a1c2-a95c2b472eb1")]
    [Route("ficha-clinica")]
    public class FichaClinicaController : ControllerBase
    {
        private readonly IFichaClinicaRepository _repository; 
        private readonly IConfiguration _configuration;

        public FichaClinicaController(IFichaClinicaRepository repository, IConfiguration configuration)  
        {
            _repository = repository;
            _configuration = configuration;
        }      
     
        [Route("get-by-id/{userId}")]
        [HttpGet]
        ///get
        public async Task<IActionResult> GetById([FromServices] IFichaClinicaRepository repository, [FromRoute] Guid userId)
        {
            return Ok(new { Data = repository.GetById(userId) });
        }

        [Route("")]
        [HttpPost]
        ///post
        public async Task<GenericCommandResult> Create([FromBody] CriarFichaClinica.Request command, [FromServices] FichaClinicaHandler handler)
        {
            return await handler.Handle(command);
        }


        [Route("")]
        [HttpPut]
        ///post
        public async Task<CriarFichaClinica.Response> Update([FromBody] CriarFichaClinica.Request command, [FromServices] IFichaClinicaRepository repository)
        {
            return await repository.Update(command);
        }


    }
}