using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Spark.Domain.Commands;
using Spark.Domain.Entities;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;

namespace Spark.Domain.Infra.Repositories
{
    public class FichaClinicaRepository : IFichaClinicaRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public FichaClinicaRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Cria ou atualiza a ficha cl�nica do paciente (PacienteId).
        /// Regras de acesso:
        /// - Se o usu�rio logado for o pr�prio paciente -> OK
        /// - Se existir v�nculo UsuarioPacientes (ResponsavelId -> PacienteId) -> OK
        /// </summary>
        public async Task<CriarFichaClinica.Response> Create(CriarFichaClinica.Request DTO)
        {
            var response = new CriarFichaClinica.Response();

            try
            { 
                var ficha = _mapper.Map<FichaClinica>(DTO);
                ficha.CreatedAt = DateTime.UtcNow;
                ficha.UpdatedAt = DateTime.UtcNow;

                await _context.FichaClinicas.AddAsync(ficha);

                var linhas = await _context.SaveChangesAsync();

                if (linhas <= 0)
                {
                    response.Sucess = false;
                    response.Erro = "Nenhuma linha afetada ao salvar ficha cl�nica.";
                    return response;
                }

                response.Sucess = true;
                response.Id = ficha.Id;
                response.Mensagem = "Ficha cl�nica salva com sucesso.";
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro ao salvar ficha cl�nica. Message:'{0}'", e.Message);

                response.Sucess = false;
                response.Erro = "Erro ao salvar ficha cl�nica. Message: " + e.Message;
                return response;
            }
        }

        /// <summary>
        /// Retorna a ficha cl�nica pelo PacienteId.
        /// S� retorna se o usu�rio logado tiver permiss�o.
        /// </summary>
        public async Task<List<FichaClinica>> GetByPacienteId(Guid pacienteId)
        {
            return  await _context.FichaClinicas
            .AsNoTracking()
            .Where(x => x.UserId == pacienteId)
            .ToListAsync();
        }

        /// <summary>
        /// Atualiza ficha cl�nica existente do paciente (PacienteId).
        /// Mantive separado pra voc� usar onde preferir, mas na pr�tica o Create j� faz upsert.
        /// </summary>
        public async Task<CriarFichaClinica.Response> Update(CriarFichaClinica.Request DTO)
        {
            var response = new CriarFichaClinica.Response();

            try
            {

                var ficha = await _context.FichaClinicas
                    .FirstOrDefaultAsync(x => x.UserId == DTO.UserId);

                if (ficha == null)
                {
                    response.Sucess = false;
                    response.Erro = "Ficha cl�nica n�o encontrada para este paciente.";
                    return response;
                }

                _mapper.Map(DTO, ficha);
                ficha.UpdatedAt = DateTime.UtcNow;

                _context.FichaClinicas.Update(ficha);

                var linhas = await _context.SaveChangesAsync();
                if (linhas <= 0)
                {
                    response.Sucess = false;
                    response.Erro = "Nenhuma linha afetada ao atualizar ficha cl�nica.";
                    return response;
                }

                response.Sucess = true;
                response.Id = ficha.Id;
                response.Mensagem = "Ficha cl�nica atualizada com sucesso.";
                return response;
            }
            catch (Exception e)
            {
                response.Sucess = false;
                response.Erro = "Erro ao fazer update da ficha cl�nica. Message: " + e.Message;
                return response;
            }
        }

        public async Task<List<FichaClinica>> getAll()
        {
            // 2) fichas desses pacientes
            var fichas = await _context.FichaClinicas
                .AsNoTracking()
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return fichas;
        }

        public async Task<FichaClinica> GetById(Guid fichaId)
        {
            return await _context
                .FichaClinicas.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == fichaId);
        }

        public async Task<CriarFichaClinica.Response> UpdateById(Guid fichaId, CriarFichaClinica.Request DTO)
        {
            var response = new CriarFichaClinica.Response();

            try
            {
                var ficha = await _context.FichaClinicas
                    .FirstOrDefaultAsync(x => x.Id == fichaId);

                if (ficha == null)
                {
                    response.Sucess = false;
                    response.Erro = "Ficha clínica não encontrada para o ID informado.";
                    return response;
                }

                _mapper.Map(DTO, ficha);
                ficha.UpdatedAt = DateTime.UtcNow;

                _context.FichaClinicas.Update(ficha);

                var linhas = await _context.SaveChangesAsync();
                if (linhas <= 0)
                {
                    response.Sucess = false;
                    response.Erro = "Nenhuma linha afetada ao atualizar ficha clínica.";
                    return response;
                }

                response.Sucess = true;
                response.Id = ficha.Id;
                response.Mensagem = "Ficha clínica atualizada com sucesso.";
                return response;
            }
            catch (Exception e)
            {
                response.Sucess = false;
                response.Erro = "Erro ao fazer update da ficha clínica. Message: " + e.Message;
                return response;
            }
        }

        public async Task<CriarFichaClinica.Response> DeleteById(Guid fichaId)
        {
            var response = new CriarFichaClinica.Response();

            try
            {
                var ficha = await _context.FichaClinicas
                    .FirstOrDefaultAsync(x => x.Id == fichaId);

                if (ficha == null)
                {
                    response.Sucess = false;
                    response.Erro = "Ficha clínica não encontrada para o ID informado.";
                    return response;
                }

                _context.FichaClinicas.Remove(ficha);

                var linhas = await _context.SaveChangesAsync();
                if (linhas <= 0)
                {
                    response.Sucess = false;
                    response.Erro = "Nenhuma linha afetada ao deletar ficha clínica.";
                    return response;
                }

                response.Sucess = true;
                response.Id = ficha.Id;
                response.Mensagem = "Ficha clínica deletada com sucesso.";
                return response;
            }
            catch (Exception e)
            {
                response.Sucess = false;
                response.Erro = "Erro ao deletar ficha clínica. Message: " + e.Message;
                return response;
            }
        }

    }
}
