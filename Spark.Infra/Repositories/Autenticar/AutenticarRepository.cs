using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Spark.Domain.Commands.Autenticar;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories.Autenticar;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System;
using Spark.Infra.Repositories.Services;
using Microsoft.Extensions.Configuration;
using Spark.Domain.Services;
using Spark.Domain.Commands;

namespace Spark.Domain.Infra.Repositories.Autenticar
{
    public class AutenticarRepository : IAutenticarRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<Usuario> _userManager;
        private IMailService _mailService;
        private IConfiguration _configuration;
        private ITokenService _tokenServiceJWT;


        public AutenticarRepository(DataContext context, IMapper mapper, UserManager<Usuario> userManager, IMailService mailService, IConfiguration configuration, ITokenService tokenServiceJWT)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _mailService = mailService;
            _configuration = configuration;
            _tokenServiceJWT = tokenServiceJWT;
        }

        public async Task<EfetuarLogin.Response> GetByUserLogin(EfetuarLogin.Request request)
        {
            EfetuarLogin.Response response = new EfetuarLogin.Response();

            // Mesma mensagem para email inexistente e senha errada (não revela se a conta existe)
            const string erroLogin = "Email ou senha inválidos.";

            if (string.IsNullOrWhiteSpace(request?.Email) || string.IsNullOrEmpty(request.Senha))
            {
                response.Erro = erroLogin;
                response.Mensagem = erroLogin;
                return response;
            }

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Senha))
            {
                response.Erro = erroLogin;
                response.Mensagem = erroLogin;
                return response;
            }

            var token = _tokenServiceJWT.GenerateToken(user);

            response.Token = token;
            response.Sucess = true;
            response.Id = user.Id;
            response.Nome = user.Nome;
            response.Email = user.Email;
            response.Mensagem = "Usuario logado com sucesso!";

            return response;
        }

        public async Task<AutenticarEmail.Response> AutenticarEmail(AutenticarEmail.Request usuario)
        {
            var user = await _userManager.FindByIdAsync(usuario.UsuarioId.ToString());

            var decodedToken = WebEncoders.Base64UrlDecode(usuario.Token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);
            try
            {
                await _userManager.ConfirmEmailAsync(user, normalToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return new AutenticarEmail.Response();
        }

        public async Task<EsqueciMinhaSenha.Response> EsqueciMinhaSenha(EsqueciSenha.Request request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    Console.WriteLine($"Usuário com e-mail {request.Email} não encontrado.");
                    // mesma resposta do caso de sucesso: não revela se o email existe
                    return new EsqueciMinhaSenha.Response { Email = request.Email };
                }

                // Gera token de recuperação de senha
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var validToken = GerarTokenBase64(token);

                // Monta a URL de recuperação
                string url = $"{_configuration["AppFrontUrl"]}{_configuration["EsqueciSenhaUrl"]}?token={Uri.EscapeDataString(validToken)}&email={Uri.EscapeDataString(request.Email)}";

                // Envia e-mail de recuperação
                await EnviarEmailRecuperacaoSenha(request.Email, url);

                // nunca devolver o token na resposta: só quem recebe o email pode resetar
                return new EsqueciMinhaSenha.Response
                {
                    Email = request.Email
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a recuperação de senha: {ex.Message}");
                return null; 
            }
        }

        private async Task EnviarEmailRecuperacaoSenha(string email, string url)
        {
            string corpoEmail =
                $"<h1>Instruções para resetar sua senha</h1>" +
                $"<p>Para resetar sua senha, clique no link abaixo:</p><br />" +
                $"<a href='{url}' " +
                    $"style=\"display: inline-flex; align-items: center; justify-content: center; " +
                    $"padding: 10px 20px; background-color: #FF5733; color: white; font-size: 15px; " +
                    $"font-weight: bold; border-radius: 30px; text-decoration: none; text-align: center; " +
                    $"box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.3);\">" +
                    $"Resetar Senha</a>";

            await _mailService.SendEmailAsync(
                _configuration["VerifiedSenderEmail"],
                email,
                "Resetar sua senha",
                corpoEmail
            );
        }

        private string GerarTokenBase64(string token)
        {
            var encodedToken = Encoding.UTF8.GetBytes(token);
            return WebEncoders.Base64UrlEncode(encodedToken);
        }


        public async Task<ResetarSenha.Response> ResetarSenha(ResetarSenha.Request model)
        {
            var falha = new ResetarSenha.Response { Message = "Erro ao alterar senha", IsSuccess = false };

            if (string.IsNullOrWhiteSpace(model?.Email) || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.NovaSenha))
                return falha;

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return falha;

            string normalToken;
            try
            {
                normalToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            }
            catch (FormatException)
            {
                return falha;
            }

            var result = await _userManager.ResetPasswordAsync(user, normalToken, model.NovaSenha);

            if (result.Succeeded)
            {
                return new ResetarSenha.Response
                {
                    Message = "Senha alterada com sucesso!",
                    IsSuccess = true,
                };
            }                   

            return new ResetarSenha.Response
            {
                Message = "Erro ao alterar senha",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description),
            };   
        }


        public HealthCheck.Response HealthCheck()
        {
            var response = new HealthCheck.Response();
            response.mensagem = "Api ok";
            return response;
        }
    }
}