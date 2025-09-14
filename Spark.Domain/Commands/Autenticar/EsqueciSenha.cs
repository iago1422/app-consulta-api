using Flunt.Notifications;
using Flunt.Validations;
using System;
using Spark.Domain.Commands.Contracts;
using System.Collections.Generic;

namespace Spark.Domain.Commands.Autenticar
{
    public class EsqueciSenha
    {
        public class Request : Notifiable, ICommand
        {
            public string Email { get; set; }

            public void Validate()
            {
                AddNotifications(
                    new Contract()
                        .Requires()
                        .IsNotNullOrEmpty(Email, "Email", "Não pode ser vazio ou nulo. ")
                );
            }
        }

        public class Response {
            public string Message { get; set; }
            public bool IsSuccess { get; set; }
            public IEnumerable<string> Errors { get; set; }
        }
    }
}
