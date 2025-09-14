using Spark.Domain.Entities.Usuarios;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark.Domain.Entities
{
    public class MovimentacaoCredito : Entity
    {
        // Usuário dono da movimentação
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Usuario Usuario { get; set; }

        /// <summary>
        /// Valor da movimentação (positivo para COMPRA, negativo para USO).
        /// </summary>
        public double Valor { get; set; }

        /// <summary>
        /// Tipo da movimentação: "COMPRA" ou "USO".
        /// </summary>
        public string Tipo { get; set; }

        /// <summary>
        /// Referência externa para idempotência (ex: pagamento_id ou consulta_id).
        /// </summary>
        public string Referencia { get; set; }

        /// <summary>
        /// Data de criação.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data de última atualização.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
