using Spark.Domain.Entities.Usuarios;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark.Domain.Entities
{
    public class FilaAtendimento : Entity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid PacienteId { get; set; }

        public string Status { get; set; } = "WAITING"; // WAITING | CALLED | DONE

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CalledAt { get; set; }
    }
}
