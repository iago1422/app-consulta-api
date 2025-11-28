using Spark.Domain.Entities.Usuarios;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark.Domain.Entities
{
    public class Consulta : Entity
    {
        // Usuário
        public Guid DoutorId { get; set; }

        [ForeignKey(nameof(DoutorId))]
        public Usuario UsuarioDoutor { get; set; }

        public Guid PacienteId { get; set; }

        [ForeignKey(nameof(PacienteId))]
        public Usuario UsuarioPaciente { get; set; }
        public string Status { get; set; } = "OPEN"; // OPEN, FINISHED, CANCELLED etc
        // Datas sistema
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
