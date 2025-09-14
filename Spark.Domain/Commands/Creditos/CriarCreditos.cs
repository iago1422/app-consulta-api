using System;
using Flunt.Notifications;
using Flunt.Validations;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands
{
    public class CriarCreditos
    {
        public class Request : Notifiable, ICommand
        {
            public Guid UserId { get; set; }
            public double Saldo { get; set; }
            
            public void Validate()
            {
                AddNotifications(
                    new Contract()
                        .Requires()                                                
                        .IsNotNull(UserId, "UserId", "Não pode ser vazio ou nulo. ")
                        .IsNotNull(Saldo, "Saldo", "Não pode ser vazio ou nulo. ")
                );
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