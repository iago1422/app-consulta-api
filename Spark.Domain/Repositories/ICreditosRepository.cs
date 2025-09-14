using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands;
using Spark.Domain.Entities;


namespace Spark.Domain.Repositories
{
    public interface ICreditosRepository
    {
        Task<CriarCreditos.Response> Create(CriarCreditos.Request objeto);
        ContaCredito GetById(Guid id);
    }
}