using System;
using Flunt.Notifications;
using Flunt.Validations;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands
{
    public class CriarAnamnese
    {
        public class Request : Notifiable, ICommand
        {
            public Guid UserId { get; set; }
            public string SintomaPrincipal { get; set; }

            // --- Sintomas (checkboxes)
            public bool TemFebre { get; set; } = false;
            public bool TemTosse { get; set; } = false;
            public bool TemFaltaDeAr { get; set; } = false;
            public bool TemDorDeCabeca { get; set; } = false;
            public bool TemNausea { get; set; } = false;
            public bool TemDiarreia { get; set; } = false;
            public bool TemDorNoPeito { get; set; } = false;
            public bool TemTontura { get; set; } = false;
            public bool TemLesoesNaPele { get; set; } = false;

            // --- Quando começaram os sintomas
            public DateTime? SintomasIniciaramEm { get; set; }

            // --- Medicamentos
            public bool TomouMedicacao { get; set; } = false;
            public string Medicacoes { get; set; }

            // --- Condições pré-existentes
            public bool TemDiabetes { get; set; } = false;
            public bool TemHipertensao { get; set; } = false;
            public bool TemAsma { get; set; } = false;
            public bool TemDoencaCardiaca { get; set; } = false;
            public bool TemDoencaPulmonarCronica { get; set; } = false;
            public bool Imunossuprimido { get; set; } = false;
            public bool TemAlergias { get; set; } = false;
            public bool EstaGravida { get; set; } = false;
            public bool OutroPreexistente { get; set; } = false;
            public string OutroPreexistenteDescricao { get; set; }

            // --- Informações de risco
            public bool TeveContatoComDoente { get; set; } = false;
            public bool TeveViagemRecente { get; set; } = false;
            public string ViagemRecenteDescricao { get; set; }
            public bool TeveTraumaRecente { get; set; } = false;
            public bool TrabalhaEmSaude { get; set; } = false;
            public bool Fumante { get; set; } = false;
            public bool UsoDeAlcool { get; set; } = false;


            public void Validate()
            {
                var contract = new Contract()
                    .Requires()
                    .IsNotNull(UserId, "UserId", "Não pode ser vazio ou nulo. ")
                    .IsNotNullOrEmpty(
                        SintomaPrincipal,
                        nameof(SintomaPrincipal),
                        "Sintoma principal não pode ser vazio ou nulo."
                    )
                    .IsNotNull(
                        SintomasIniciaramEm,
                        nameof(SintomasIniciaramEm),
                        "A data de início dos sintomas não pode ser vazia ou nula."
                    );

                if (TomouMedicacao)
                {
                    contract.IsNotNullOrEmpty(
                        Medicacoes,
                        nameof(Medicacoes),
                        "Descreva as medicações utilizadas."
                    );
                }

                if (OutroPreexistente)
                {
                    contract.IsNotNullOrEmpty(
                        OutroPreexistenteDescricao,
                        nameof(OutroPreexistenteDescricao),
                        "Descreva a condição pré-existente."
                    );
                }

                if (TeveViagemRecente)
                {
                    contract.IsNotNullOrEmpty(
                        ViagemRecenteDescricao,
                        nameof(ViagemRecenteDescricao),
                        "Descreva a viagem recente."
                    );
                }

                AddNotifications(contract);
            }

        }

        public class Response
        {
            public Guid Id { get; set; }
            public bool Sucess { get; set; } = false;
            public string Mensagem { get; set; }
            public string Erro { get; set; }
        }
    }
}