using System;
using Flunt.Notifications;
using Flunt.Validations;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands
{
    public class JoinQueueRequest
    {
        public class Request : Notifiable, ICommand
        {
            public Guid TenantId { get; set; }
            public Guid AnamneseId { get; set; }
            public Guid FichaId { get; set; }

            public void Validate()
            {
                var contract = new Contract()
                    .Requires()
                    .IsNotNull(TenantId, "TenantId", "Não pode ser vazio ou nulo. ")
                    .IsNotNull(AnamneseId, "AnamneseId", "Não pode ser vazio ou nulo. ")                                  
                    .IsNotNull(FichaId, "AnamneseId", "Não pode ser vazio ou nulo. ");                                    
            
                AddNotifications(contract);
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