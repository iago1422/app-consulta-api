using Flunt.Notifications;
using Flunt.Validations;
using System;
using Spark.Domain.Commands.Contracts;
using System.Text.Json.Serialization;

namespace Spark.Domain.Commands.Usuario
{
    public class DeletarUsuario
    {
        public class Request : Notifiable, ICommand
        {
            public Guid Id { get; set; }   

            public void Validate()
            {
                AddNotifications(
                    new Contract()
                        .Requires()
                        .IsNotNull(Id, "Id", "Não pode ser vazio ou nulo. "));            
            }
        }
       
        public class Response {
            public bool Sucess { get; set; } = false;
            public string Mensagem { get; set; }
            public string Erro { get; set; }
        }
    }
}
