using Flunt.Notifications;
using Flunt.Validations;
using System;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands.Autenticar
{
    public class AutenticarEmail
    {
        public class Request : Notifiable, ICommand
        {
            public Guid UsuarioId { get; set; }
            public string Token { get; set; }

            public void Validate()
            {
                AddNotifications(
                    new Contract()
                        .Requires()
                        .IsNotNull(UsuarioId, "Email", "Não pode ser vazio ou nulo. ")
                        .IsNotNullOrEmpty(Token, "Token", "Não pode ser vazio ou nulo. ")
                );
            }
        }

        public class Response {
           
        }
    }
}
