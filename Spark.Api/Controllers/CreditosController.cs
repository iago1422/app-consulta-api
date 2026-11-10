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
using Spark.Domain.Services;
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

        // POST /creditos removido: qualquer paciente logado conseguia se dar créditos.
        // Crédito só entra pelo webhook do Mercado Pago (que chama o repositório direto).

        [Route("")]
        [HttpPut]
        ///put (débito)
        public async Task<IActionResult> Update([FromBody] CriarCreditos.Request command, [FromServices] ICreditosRepository repository, [FromServices] ICustoConsulta custos)
        {
            if (command == null)
                return BadRequest(new CriarCreditos.Response { Erro = "O valor a debitar deve ser maior que zero." });

            // Consulta da fila: o custo vem da configuração (painel), não do valor que o portal manda
            var custo = await custos.CreditosDaFila(command.Referencia);
            if (custo.HasValue)
                command.Saldo = custo.Value;

            if (command.Saldo <= 0)
                return BadRequest(new CriarCreditos.Response { Erro = "O valor a debitar deve ser maior que zero." });

            var result = await repository.Update(command);

            // 409 em falha (saldo insuficiente, conta inexistente, erro): o portal trata 200 como sucesso
            if (!result.Sucess)
                return Conflict(result);

            result.ValorDebitado = command.Saldo;
            return Ok(result);
        }


    }
}