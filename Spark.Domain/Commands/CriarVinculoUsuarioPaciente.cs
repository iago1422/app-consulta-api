using System;
using Flunt.Notifications;
using Flunt.Validations;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands
{
    public class CriarVinculoUsuarioPaciente
    {
        public class Request : Notifiable, ICommand
        {
            public Guid PacienteId { get; set; }
            public string TipoVinculo { get; set; } // MAE, PAI, TUTOR, PROPRIO
            public bool IsResponsavelLegal { get; set; }

            // não vem do client
            public Guid UsuarioLogadoId { get; set; }

            public void Validate()
            {
                AddNotifications(new Contract()
                    .Requires()
                    .IsNotEmpty(PacienteId, "PacienteId", "PacienteId é obrigatório")
                    .IsNotNullOrEmpty(TipoVinculo, "TipoVinculo", "Tipo de vínculo é obrigatório")
                );
            }
        }

        public class Response
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }
    }
}
