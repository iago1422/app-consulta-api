using Spark.Domain.Entities.Usuarios;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark.Domain.Entities
{
    public class ContaCredito : Entity
    {
        // Usuário dono da conta de créditos
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Usuario Usuario { get; set; }

        /// <summary>
        /// Saldo atual em créditos do usuário.
        /// </summary>
        public double Saldo { get; set; }

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
