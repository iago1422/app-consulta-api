using Flunt.Notifications;
using Flunt.Validations;
using System;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands.Usuario
{
    public class AtualizarUsuario
    {
        public class Request : Notifiable, ICommand
        {
            public Guid Id { get; set; }
            public string Nome { get; set; }
            public string Email { get; set; }
            public Guid PlanoId { get; set; }
            public string Imagem { get; set; }
            public string Cpf { get; set; }
            public string Rg { get; set; }
            public void Validate()
            {
                AddNotifications(
                    new Contract()
                        .Requires()
                        .IsNotNull(Id, "Id", "Não pode ser vazio ou nulo. ")
                );
            }
        }

        public class Response {
            public bool Sucess { get; set; } = false;
            public string Mensagem { get; set; }
            public string Erro { get; set; }
        }
    }
}
