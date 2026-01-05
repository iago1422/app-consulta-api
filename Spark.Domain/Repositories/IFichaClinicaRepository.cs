using System;
using System.Threading.Tasks;
using Spark.Domain.Commands;
using Spark.Domain.Entities;

namespace Spark.Domain.Repositories
{
    public interface IFichaClinicaRepository
    {
        /// <summary>
        /// Cria ou atualiza a ficha clínica do paciente.
        /// O acesso é validado com base no usuário logado (paciente ou responsável).
        /// </summary>
        Task<CriarFichaClinica.Response> Create(
            CriarFichaClinica.Request objeto
        );

        /// <summary>
        /// Retorna a ficha clínica pelo PacienteId,
        /// validando se o usuário logado pode acessá-la.
        /// </summary>
        Task<FichaClinica> GetByPacienteId(
            Guid pacienteId,
            Guid usuarioLogadoId
        );

        /// <summary>
        /// Atualiza a ficha clínica existente do paciente,
        /// validando vínculo ou acesso direto.
        /// </summary>
        Task<CriarFichaClinica.Response> Update(
            CriarFichaClinica.Request objeto
        );
    }
}
