using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark.Domain.Entities
{
    public class Conexao
    {
        /// <summary>
        /// ID incremental (SERIAL no banco).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID da chamada associada.
        /// </summary>
        public Guid ChamadaId { get; set; }

        [ForeignKey(nameof(ChamadaId))]
        public Chamada Chamada { get; set; }

        /// <summary>
        /// Identificador do remetente (caller ou callee).
        /// </summary>
        public string Tipo { get; set; }

        /// <summary>
        /// Objeto ICE Candidate (JSONB).
        /// </summary>
        public string Dispositivo { get; set; }


        /// <summary>
        /// Data de criação da conta de créditos.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data da última atualização de saldo.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
