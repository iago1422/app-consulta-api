using System;
using System.Collections.Generic;
using System.Text;
using Spark.Domain.Entities.Usuarios;

namespace Spark.Domain.Services
{
    public interface ITokenService
    {
        public string GenerateToken(Usuario user);
    }
}
