using System;
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
        /// Cria ou atualiza a ficha clínica do usuário.
        /// Se já existir ficha para o UserId, ela é atualizada.
        /// </summary>
        public async Task<CriarFichaClinica.Response> Create(CriarFichaClinica.Request DTO)
        {
            var response = new CriarFichaClinica.Response();

            try
            {
                // Verifica se já existe ficha clínica para esse usuário
                var ficha = await _context.FichaClinicas
                    .FirstOrDefaultAsync(x => x.UserId == DTO.UserId);

                if (ficha == null)
                {
                    // Não existe -> cria nova
                    ficha = _mapper.Map<FichaClinica>(DTO);
                    ficha.CreatedAt = DateTime.UtcNow;
                    ficha.UpdatedAt = DateTime.UtcNow;

                    await _context.FichaClinicas.AddAsync(ficha);
                }
                else
                {
                    // Já existe -> atualiza a ficha
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
        /// Retorna a ficha clínica pelo UserId.
        /// </summary>
        public FichaClinica GetById(Guid userId)
        {
            return _context
                .FichaClinicas.AsNoTracking()
                .FirstOrDefault(x => x.UserId == userId);
        }

        /// <summary>
        /// Atualiza a ficha clínica existente de um usuário.
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
                    response.Erro = "Ficha clínica não encontrada para este usuário.";
                    return response;
                }

                // Atualiza campos
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
    }
}
