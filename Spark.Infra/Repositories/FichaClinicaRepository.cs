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
        /// Cria ou atualiza a ficha clínica do paciente (PacienteId).
        /// Regras de acesso:
        /// - Se o usuário logado for o próprio paciente -> OK
        /// - Se existir vínculo UsuarioPacientes (ResponsavelId -> PacienteId) -> OK
        /// </summary>
        public async Task<CriarFichaClinica.Response> Create(CriarFichaClinica.Request DTO)
        {
            var response = new CriarFichaClinica.Response();

            try
            {
                // 1) valida permissão
                var permitido = await PodeAcessarPaciente(DTO.UsuarioLogadoId, DTO.UserId);
                if (!permitido)
                {
                    response.Sucess = false;
                    response.Erro = "Acesso negado: usuário não tem vínculo com o paciente.";
                    return response;
                }

                // 2) upsert por PacienteId
                var ficha = await _context.FichaClinicas
                    .FirstOrDefaultAsync(x => x.UserId == DTO.UserId);

                if (ficha == null)
                {
                    ficha = _mapper.Map<FichaClinica>(DTO);
                    ficha.CreatedAt = DateTime.UtcNow;
                    ficha.UpdatedAt = DateTime.UtcNow;

                    await _context.FichaClinicas.AddAsync(ficha);
                }
                else
                {
                    _mapper.Map(DTO, ficha);
                    ficha.UpdatedAt = DateTime.UtcNow;

                    _context.FichaClinicas.Update(ficha);
                }

                var linhas = await _context.SaveChangesAsync();

                if (linhas <= 0)
                {
                    response.Sucess = false;
                    response.Erro = "Nenhuma linha afetada ao salvar ficha clínica.";
                    return response;
                }

                response.Sucess = true;
                response.Id = ficha.Id;
                response.Mensagem = "Ficha clínica salva com sucesso.";
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro ao salvar ficha clínica. Message:'{0}'", e.Message);

                response.Sucess = false;
                response.Erro = "Erro ao salvar ficha clínica. Message: " + e.Message;
                return response;
            }
        }

        /// <summary>
        /// Retorna a ficha clínica pelo PacienteId.
        /// Só retorna se o usuário logado tiver permissão.
        /// </summary>
        public async Task<FichaClinica> GetByPacienteId(Guid pacienteId, Guid usuarioLogadoId)
        {
            var permitido = await PodeAcessarPaciente(usuarioLogadoId, pacienteId);
            if (!permitido) return null;

            return await _context
                .FichaClinicas.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == pacienteId);
        }

        /// <summary>
        /// Atualiza ficha clínica existente do paciente (PacienteId).
        /// Mantive separado pra você usar onde preferir, mas na prática o Create já faz upsert.
        /// </summary>
        public async Task<CriarFichaClinica.Response> Update(CriarFichaClinica.Request DTO)
        {
            var response = new CriarFichaClinica.Response();

            try
            {
                var permitido = await PodeAcessarPaciente(DTO.UsuarioLogadoId, DTO.UserId);
                if (!permitido)
                {
                    response.Sucess = false;
                    response.Erro = "Acesso negado: usuário não tem vínculo com o paciente.";
                    return response;
                }

                var ficha = await _context.FichaClinicas
                    .FirstOrDefaultAsync(x => x.UserId == DTO.UserId);

                if (ficha == null)
                {
                    response.Sucess = false;
                    response.Erro = "Ficha clínica não encontrada para este paciente.";
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

        /// <summary>
        /// Regra central de permissão:
        /// - o próprio paciente pode acessar
        /// - ou responsável vinculado pode acessar
        /// </summary>
        private async Task<bool> PodeAcessarPaciente(Guid usuarioLogadoId, Guid pacienteId)
        {
            // Próprio paciente
            if (usuarioLogadoId == pacienteId)
                return true;

            // Responsável vinculado ao paciente
            return await _context.UsuarioPacientes.AsNoTracking()
                .AnyAsync(x => x.ResponsavelId == usuarioLogadoId && x.PacienteId == pacienteId);
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

    }
}
