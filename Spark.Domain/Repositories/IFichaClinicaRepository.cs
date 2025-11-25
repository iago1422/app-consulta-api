using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands;
using Spark.Domain.Entities;


namespace Spark.Domain.Repositories
{
    public interface IFichaClinicaRepository
    {
        Task<CriarFichaClinica.Response> Create(CriarFichaClinica.Request objeto);
        FichaClinica GetById(Guid id);
        Task<CriarFichaClinica.Response> Update(CriarFichaClinica.Request objeto);

    }
}