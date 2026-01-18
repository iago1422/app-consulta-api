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
using MercadoPago.Resource.User;

namespace Spark.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "acb3830a-402b-4865-9f70-7b28d39f66ad,efe703c2-2076-4d5b-aa04-aaee90953e49,e0e938ef-5ca3-48bc-b5a5-30a0fd0b64ca")]
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
     
        [Route("get-by-id/{userid}")]
        [HttpGet]
        ///get
        public async Task<IActionResult> GetById([FromServices] ICreditosRepository repository, [FromRoute] Guid userid)
        {
            return Ok(new { Data = repository.GetById(userid) });
        }

        [Route("")]
        [HttpPost]
        ///post
        public async Task<GenericCommandResult> Create([FromBody] CriarCreditos.Request command, [FromServices] CreditosHandler handler)
        {
            return await handler.Handle(command);
        }

        [Route("")]
        [HttpPut]
        ///post
        public async Task<CriarCreditos.Response> Update([FromBody] CriarCreditos.Request command, [FromServices] ICreditosRepository repository)
        {
            return await repository.Update(command);
        }


    }
}