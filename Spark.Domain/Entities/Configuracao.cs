using System;
using System.ComponentModel.DataAnnotations;

namespace Spark.Domain.Entities
{
    /// <summary>Configuração chave → JSON editada pelo painel (hoje só "consultas").</summary>
    public class Configuracao
    {
        [Key]
        public string Chave { get; set; }
        public string Valor { get; set; }
        public DateTime? AtualizadoEm { get; set; }
        public Guid? AtualizadoPor { get; set; }
    }
}
