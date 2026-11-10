using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Services;

namespace Spark.Api.Services
{
    public class TokenService : ITokenService
    {
        // O token dura 2h, mas um token vencido há até 30 dias pode ser trocado por um novo
        // (POST /autenticar/renovar). Quem usa o portal todo dia nunca precisa digitar a senha.
        // Trocar a senha muda o SecurityStamp e derruba todas as sessões.
        public static readonly TimeSpan JanelaRenovacao = TimeSpan.FromDays(30);
        public const string StampClaim = "stamp";

        public TokenService(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        private byte[] ChaveAssinatura() =>
            Encoding.ASCII.GetBytes(Configuration.GetSection("AuthenticationJWT").GetValue<string>("secretKey"));

        public string HashStamp(string securityStamp) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(securityStamp ?? string.Empty)));

        public string GenerateToken(Usuario user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = ChaveAssinatura();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    // ID do usuário (resolve seu problema)
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                    // dados úteis
                    new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),

                    // role/perfil
                    new Claim(ClaimTypes.Role, user.PerfilId.ToString()),

                    // renovação: invalida se a senha mudar
                    new Claim(StampClaim, HashStamp(user.SecurityStamp))
                        }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public (Guid UserId, string Stamp)? LerTokenRenovavel(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var parametros = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(ChaveAssinatura()),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false // vencido é o caso normal aqui; o limite é a janela abaixo
            };

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(token, parametros, out var validado);
                if (validado.ValidTo.Add(JanelaRenovacao) < DateTime.UtcNow) return null;

                var stamp = principal.FindFirst(StampClaim)?.Value;
                if (stamp == null || !Guid.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                    return null;

                return (userId, stamp);
            }
            catch (Exception ex) when (ex is SecurityTokenException || ex is ArgumentException)
            {
                return null; // assinatura inválida, token malformado
            }
        }
    }
}
