using Spark.Domain.Entities.Usuarios;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark.Domain.Entities
{
    public class FilaAtendimento : Entity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid AnamnseId { get; set; }
        public Guid FichaId { get; set; }

        public string Tipo { get; set; } = "VIDEO"; // VIDEO, CHAT, VOICE

        public string Status { get; set; } = "WAITING"; // WAITING | CALLED | DONE

        public DateTime CreatedAt { get; set; }
        public DateTime? CalledAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
