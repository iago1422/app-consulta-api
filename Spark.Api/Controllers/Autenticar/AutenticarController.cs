using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spark.Domain.Commands;
using Spark.Domain.Commands.Autenticar;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Repositories.Autenticar;
using Spark.Domain.Services;
using System;
using System.Threading.Tasks;

namespace Spark.Api.Controllers.Autenticar
{
    [ApiController]
    [Route("autenticar")]
    public class AutenticarController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        public AutenticarController(ITokenService tokenService )
        {
            _tokenService = tokenService;
        }
        [HttpPost]
        [Route("login")]
        public async Task<GenericCommandResult> Authenticate([FromBody] EfetuarLogin.Request command, [FromServices]IAutenticarRepository repository)
        {

            var result = await repository.GetByUserLogin(command);

            if (result.Sucess)
            {
                return new GenericCommandResult(result.Sucess, result.Mensagem, result); ;
            }

            return new GenericCommandResult(result.Sucess, result.Erro, result);
        }

        [Route("confirmaremail")]
        [HttpGet]
        ///get
        public async Task<GenericCommandResult> ConfimaEmail(Guid userid, string token, [FromServices] IAutenticarRepository _repository)
        {
            AutenticarEmail.Request request = new AutenticarEmail.Request();
            request.UsuarioId = userid;
            request.Token = token;
            var result =  await _repository.AutenticarEmail(request);

            return new GenericCommandResult(true, "Email Confirmado", result);
        }

        [Route("esqueciminhasenha")]
        [HttpPost]
        ///post
        public async Task<GenericCommandResult> EsqueciMinhaSenha(EsqueciSenha.Request request, [FromServices] IAutenticarRepository _repository)
        {
            var result = await _repository.EsqueciMinhaSenha(request);
            return new GenericCommandResult(true, "Solicitação enviada!", result); ;
        }

        [Route("resetarsenha")]
        [HttpPost]
        ///post
        public async Task<GenericCommandResult> ResetarSenha(ResetarSenha.Request request, [FromServices] IAutenticarRepository _repository)
        {
            var result = await _repository.ResetarSenha(request);

            if (result.IsSuccess)
            {
                return new GenericCommandResult(true, "Senha alterada", result); ;
            }

            return new GenericCommandResult(false, "Falha ao alterar senha", result);
        }

        [Route("health")]
        [HttpGet]
        ///get
        public async Task<GenericCommandResult> HealfhCheck([FromServices] IAutenticarRepository _repository)
        {
            var result = _repository.HealthCheck();
            return new GenericCommandResult(true, "HealhCheck: OK ", result);
        }

    }
}
