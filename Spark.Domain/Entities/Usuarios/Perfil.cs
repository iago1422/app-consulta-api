using Microsoft.AspNetCore.Identity;
using System;

namespace Spark.Domain.Entities.Usuarios
{
    public class Perfil : IdentityRole<Guid>
    {
        public bool Administrador { get; set; }
    }
}
