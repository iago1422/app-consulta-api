using Flunt.Notifications;
using Flunt.Validations;
using System;
using Spark.Domain.Commands.Contracts;
using System.Text.Json.Serialization;

namespace Spark.Domain.Commands.Usuario
{
    public class CriarUsuario
    {
        public class RequestBanco : Notifiable, ICommand
        {
            public string Nome { get; set; }
            public string Email { get; set; }
            public string Senha { get; set; }
            public string Cpf { get; set; }
            public Guid PerfilId { get; set; }
            public string Imagem { get; set; }

            public void Validate()
            {
                AddNotifications(
                    new Contract()
                        .Requires()
                        .IsNotNullOrEmpty(Nome, "Nome", "Não pode ser vazio ou nulo. ")
                        .IsNotNullOrEmpty(Email, "Email", "Não pode ser vazio ou nulo. ")
                        .IsEmail(Email, "Email", "Email inválido. ")
                        .IsNotNullOrEmpty(Senha, "Senha", "Não pode ser vazio ou nulo. ")
                        //.IsNotNull(PlanoId, "PlanoId", "Não pode ser vazio ou nulo. ")
                        .IsNotNull(PerfilId, "Perfil", "Não pode ser vazio ou nulo. ")
                );
            }
        }
        public class Request : Notifiable, ICommand
        {
            public string Nome { get; set; }
            public string Email { get; set; }
            public string Senha { get; set; }
            public string Cpf { get; set; }
            public string Perfil { get; set; }
            //public Guid PlanoId { get; set; }
            public string Imagem { get; set; }

            public void Validate()
            {
                AddNotifications(
                    new Contract()
                        .Requires()
                        .IsNotNullOrEmpty(Nome, "Nome", "Não pode ser vazio ou nulo. ")
                        .IsNotNullOrEmpty(Email, "Email", "Não pode ser vazio ou nulo. ")
                        .IsEmail(Email, "Email", "Email inválido. ")
                        .IsNotNullOrEmpty(Senha, "Senha", "Não pode ser vazio ou nulo. ")
                        //.IsNotNull(PlanoId, "PlanoId", "Não pode ser vazio ou nulo. ")
                        .IsNotNullOrEmpty(Perfil, "Perfil", "Não pode ser vazio ou nulo. ")
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
