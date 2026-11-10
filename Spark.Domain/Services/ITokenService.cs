using System;
using System.Collections.Generic;
using System.Text;
using Spark.Domain.Entities.Usuarios;

namespace Spark.Domain.Services
{
    public interface ITokenService
    {
        public string GenerateToken(Usuario user);

        /// <summary>
        /// Lê um token assinado por nós, mesmo vencido, se ainda estiver na janela de renovação.
        /// Devolve o id do usuário e o hash do security stamp, ou null se não puder ser renovado.
        /// </summary>
        public (Guid UserId, string Stamp)? LerTokenRenovavel(string token);

        /// <summary>Hash do SecurityStamp gravado no token (o stamp em si não sai no payload).</summary>
        public string HashStamp(string securityStamp);
    }
}
