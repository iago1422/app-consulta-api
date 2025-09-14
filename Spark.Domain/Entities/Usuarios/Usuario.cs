using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Spark.Domain.Entities.Usuarios
{
    public class Usuario : IdentityUser<Guid>
    {
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public Guid PerfilId { get; set; }
        [ForeignKey("PerfilId")]
        public Perfil Perfil { get; set; }
        public string Imagem { get; set; }
    }

}
