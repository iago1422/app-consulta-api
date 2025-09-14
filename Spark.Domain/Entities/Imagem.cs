using Spark.Domain.Entities.Usuarios;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Spark.Domain.Entities
{
    public class Imagem : Entity
    {
        public string Nome { get; set; }
        public string ChaveAws { get; set; }
    }
}
