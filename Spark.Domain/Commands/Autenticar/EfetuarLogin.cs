using Flunt.Notifications;
using Flunt.Validations;
using System;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands.Autenticar
{
    public class EfetuarLogin
    {
        public class Request : Notifiable, ICommand
        {
            public string Email { get; set; }
            public string Senha { get; set; }

            public void Validate()
            {
                AddNotifications(
                    new Contract()
                        .Requires()
                        .IsNotNullOrEmpty(Email, "Email", "Não pode ser vazio ou nulo. ")
                        .IsEmail(Email, "Email", "Email inválido. ")
                        .IsNotNullOrEmpty(Senha, "Texto", "Não pode ser vazio ou nulo. ")
                );
            }
        }

        public class Response {
            public Guid Id { get; set; } 
            public string Email { get; set; }
            public string Nome { get; set; }
            public string Token { get; set; }
            public string Mensagem { get; set; }
            public string Erro { get; set; }
            public bool Sucess { get; set; } = false;
        }
    }
}
