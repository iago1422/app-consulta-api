using System;
using Flunt.Notifications;
using Flunt.Validations;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands
{
    public class CriarImagem
    {
        public class Request : Notifiable, ICommand
        {
            public Guid AtividadeId { get; set; }
            public Guid ApontamentoId { get; set; }
            public string ChaveAws{ get; set; }
            public string Nome { get; set; }
            
            public void Validate()
            {
                AddNotifications(
                    new Contract()
                        .Requires()                                                
                        .IsNotNull(AtividadeId, "AtividadeId", "Não pode ser vazio ou nulo. ")
                        .IsNotNull(ApontamentoId, "ApontamentoId", "Não pode ser vazio ou nulo. ")
                );
            }
        }

        public class Response
        {
            public Guid Id { get; set; }
        }
    }
}