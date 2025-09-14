using System;
using Flunt.Notifications;
using Flunt.Validations;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands
{
    public class CriarImagemUsuario
    {
        public class Request : Notifiable, ICommand
        {
            public string ChaveAws { get; set; }
            public string Nome { get; set; }

            public void Validate()
            {
                AddNotifications(
                    new Contract()
                        .Requires()
                );
            }
        }

        public class Response
        {
            public Guid Id { get; set; }
            public string ChaveAws { get; set; }
        }
    }
}