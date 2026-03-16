using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spark.Domain.Commands;
using Spark.Domain.Entities;

namespace Spark.Domain.Repositories
{
    public interface IFichaClinicaRepository
    {
        /// <summary>
        /// Cria ou atualiza a ficha cl�nica do paciente.
        /// O acesso � validado com base no usu�rio logado (paciente ou respons�vel).
        /// </summary>
        Task<CriarFichaClinica.Response> Create(
            CriarFichaClinica.Request objeto
        );

        /// <summary>
        /// Retorna a ficha cl�nica pelo PacienteId,
        /// validando se o usu�rio logado pode acess�-la.
        /// </summary>
        Task<List<FichaClinica>> GetByPacienteId(
            Guid pacienteId);

        /// <summary>
        /// Atualiza a ficha cl�nica existente do paciente,
        /// validando v�nculo ou acesso direto.
        /// </summary>
        Task<CriarFichaClinica.Response> Update(
            CriarFichaClinica.Request objeto
        );


        Task<List<FichaClinica>> getAll();

        Task<FichaClinica> GetById(Guid fichaId);

        Task<CriarFichaClinica.Response> UpdateById(
            Guid fichaId,
            CriarFichaClinica.Request objeto
        );

        Task<CriarFichaClinica.Response> DeleteById(Guid fichaId);
    }
}
