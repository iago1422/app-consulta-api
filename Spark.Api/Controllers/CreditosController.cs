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
    [Route("creditos")]
    public class CreditoController : ControllerBase
    {
        private readonly ICreditosRepository _repository; 
        private readonly IConfiguration _configuration;

        public CreditoController(ICreditosRepository repository, IConfiguration configuration)  
        {
            _repository = repository;
            _configuration = configuration;
        }      
     
        [Route("get-by-id/{idatividade}")]
        [HttpGet]
        ///get
        public async Task<IActionResult> GetById([FromServices] ICreditosRepository repository, [FromRoute] Guid idatividade)
        {
            return Ok(new { Data = repository.GetById(idatividade) });
        }

        [Route("")]
        [HttpPost]
        ///post
        public async Task<GenericCommandResult> Create([FromBody] CriarCreditos.Request command, [FromServices] CreditosHandler handler)
        {
            return await handler.Handle(command);
        }



    }
}