using System;
using Flunt.Notifications;
using Flunt.Validations;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands
{
    public class HealthCheck
    {    
        public class Response
        {
            public string mensagem { get; set; }
        }
    }
}