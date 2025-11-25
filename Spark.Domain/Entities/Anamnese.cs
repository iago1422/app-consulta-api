using Spark.Domain.Entities.Usuarios;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark.Domain.Entities
{
    public class Anamnese : Entity
    {
        // Usuário
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Usuario Usuario { get; set; }

        // --- Sintoma principal
        public string SintomaPrincipal { get; set; }

        // --- Sintomas (checkboxes)
        public bool TemFebre { get; set; }
        public bool TemTosse { get; set; }
        public bool TemFaltaDeAr { get; set; }
        public bool TemDorDeCabeca { get; set; }
        public bool TemNausea { get; set; }
        public bool TemDiarreia { get; set; }
        public bool TemDorNoPeito { get; set; }
        public bool TemTontura { get; set; }
        public bool TemLesoesNaPele { get; set; }

        // --- Quando começaram os sintomas
        public DateTime? SintomasIniciaramEm { get; set; }

        // --- Medicamentos
        public bool TomouMedicacao { get; set; }
        public string Medicacoes { get; set; }

        // --- Condições pré-existentes
        public bool TemDiabetes { get; set; }
        public bool TemHipertensao { get; set; }
        public bool TemAsma { get; set; }
        public bool TemDoencaCardiaca { get; set; }
        public bool TemDoencaPulmonarCronica { get; set; }
        public bool Imunossuprimido { get; set; }
        public bool TemAlergias { get; set; }
        public bool EstaGravida { get; set; }
        public bool OutroPreexistente { get; set; }
        public string OutroPreexistenteDescricao { get; set; }

        // --- Informações de risco
        public bool TeveContatoComDoente { get; set; }
        public bool TeveViagemRecente { get; set; }
        public string ViagemRecenteDescricao { get; set; }
        public bool TeveTraumaRecente { get; set; }
        public bool TrabalhaEmSaude { get; set; }
        public bool Fumante { get; set; }
        public bool UsoDeAlcool { get; set; }

        // Datas sistema
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
