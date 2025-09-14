using System;
using Flunt.Notifications;
using Flunt.Validations;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands
{
    public class DeletarImagemUsuario
    {
        public class Request : Notifiable, ICommand
        {
            public Guid Id { get; set; }

            public void Validate()
            {
                AddNotifications(
                    new Contract()
                        .Requires()
                        .IsNotNull(Id, "Id", "Não pode ser vazio ou nulo. ")
                );
            }
        }

        public class response
        {
            public bool deletado { get; set; }
        }
    }
}