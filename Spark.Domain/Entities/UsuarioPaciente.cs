using Spark.Domain.Entities.Usuarios;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark.Domain.Entities
{
    public class UsuarioPaciente : Entity
    {
        // Quem acessa (mãe/pai/tutor)
        public Guid ResponsavelId { get; set; }

        [ForeignKey(nameof(ResponsavelId))]
        public Usuario Responsavel { get; set; }

        // Quem é atendido (filho/paciente)
        public Guid PacienteId { get; set; }

        [ForeignKey(nameof(PacienteId))]
        public Usuario Paciente { get; set; }

        // "MAE", "PAI", "TUTOR", "PROPRIO"
        public string TipoVinculo { get; set; }

        public bool IsResponsavelLegal { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
