using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands;
using Spark.Domain.Entities;


namespace Spark.Domain.Repositories
{
    public interface IAnamneseRepository
    {
        Task<CriarAnamnese.Response> Create(CriarAnamnese.Request objeto);
        Anamnese GetById(Guid id);
        Task<CriarAnamnese.Response> Update (CriarAnamnese.Request objeto);
    }
}