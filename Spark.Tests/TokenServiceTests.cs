using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Spark.Api.Services;
using Spark.Domain.Entities.Usuarios;
using Xunit;

namespace Spark.Tests
{
    public class TokenServiceTests
    {
        private const string Chave = "chave-de-teste-com-mais-de-32-bytes-para-hmac";

        private static TokenService Servico(string chave = Chave) =>
            new TokenService(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { ["AuthenticationJWT:secretKey"] = chave })
                .Build());

        private static readonly Usuario Medico = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "medico@example.com",
            PerfilId = Guid.NewGuid(),
            SecurityStamp = "stamp-original"
        };

        // Token com a mesma forma do GenerateToken, mas vencido em `venceuEm`
        private static string TokenVencido(DateTime venceuEm, string stamp, string chave = Chave)
        {
            var descritor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Medico.Id.ToString()),
                    new Claim(TokenService.StampClaim, stamp)
                }),
                IssuedAt = venceuEm.AddHours(-2),
                NotBefore = venceuEm.AddHours(-2),
                Expires = venceuEm,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(chave)), SecurityAlgorithms.HmacSha256Signature)
            };
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descritor));
        }

        [Fact]
        public void Token_recem_emitido_e_renovavel_com_stamp_do_usuario()
        {
            var servico = Servico();
            var dados = servico.LerTokenRenovavel(servico.GenerateToken(Medico));

            Assert.NotNull(dados);
            Assert.Equal(Medico.Id, dados.Value.UserId);
            Assert.Equal(servico.HashStamp("stamp-original"), dados.Value.Stamp);
            Assert.NotEqual(servico.HashStamp("stamp-novo"), dados.Value.Stamp); // senha trocada => não bate
        }

        [Theory]
        [InlineData(1, true)]    // vencido ontem: renova
        [InlineData(29, true)]
        [InlineData(31, false)]  // fora da janela de 30 dias: pede senha
        public void Token_vencido_renova_so_dentro_da_janela(int diasVencido, bool renovavel)
        {
            var servico = Servico();
            var token = TokenVencido(DateTime.UtcNow.AddDays(-diasVencido), servico.HashStamp("stamp-original"));

            Assert.Equal(renovavel, servico.LerTokenRenovavel(token) != null);
        }

        [Fact]
        public void Token_assinado_com_outra_chave_nao_renova()
        {
            var token = TokenVencido(DateTime.UtcNow.AddHours(-1), "x", chave: "outra-chave-qualquer-com-mais-de-32-bytes");
            Assert.Null(Servico().LerTokenRenovavel(token));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("nao.e.jwt")]
        public void Token_malformado_nao_renova(string token)
        {
            Assert.Null(Servico().LerTokenRenovavel(token));
        }

        [Fact]
        public void Token_sem_stamp_emitido_antes_desta_versao_nao_renova()
        {
            var descritor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Medico.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Chave)), SecurityAlgorithms.HmacSha256Signature)
            };
            var handler = new JwtSecurityTokenHandler();
            Assert.Null(Servico().LerTokenRenovavel(handler.WriteToken(handler.CreateToken(descritor))));
        }
    }
}
