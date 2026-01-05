using System;
using System.Threading.Tasks;
using Spark.Domain.Commands;
using Spark.Domain.Entities;

namespace Spark.Domain.Repositories
{
    public interface ICriarVinculoPacienteRepository
    {
        Task<bool> CriarVinculoPaciente(
            Guid responsavelId,
            Guid pacienteId,
            string tipoVinculo,
            bool isResponsavelLegal
        );
    }
}
