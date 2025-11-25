using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Spark.Domain.Commands;
using Spark.Domain.Entities;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;

namespace Spark.Domain.Infra.Repositories
{
    public class AnamneseRepository : IAnamneseRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public AnamneseRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Cria ou atualiza a anamnese de um usuário.
        /// Se já existir anamnese para o UserId, atualiza.
        /// Se não existir, cria uma nova.
        /// </summary>
        public async Task<CriarAnamnese.Response> Create(CriarAnamnese.Request DTO)
        {
            var response = new CriarAnamnese.Response();

            try
            {              

                // Verifica se já existe anamnese para esse usuário
                var anamnese = await _context.Anamneses
                    .FirstOrDefaultAsync(x => x.UserId == DTO.UserId);

                if (anamnese == null)
                {
                    // Não existe -> cria nova
                    anamnese = _mapper.Map<Anamnese>(DTO);
                    await _context.Anamneses.AddAsync(anamnese);
                }
                else
                {
                    // Já existe -> atualiza campos
                    _mapper.Map(DTO, anamnese);
                    _context.Anamneses.Update(anamnese);
                }

                var linhas = await _context.SaveChangesAsync();

                if (linhas <= 0)
                {
                    response.Sucess = false;
                    response.Erro = "Nenhuma linha afetada ao salvar anamnese.";
                    return response;
                }

                response.Sucess = true;
                response.Id = anamnese.Id;
                response.Mensagem = "Anamnese salva com sucesso.";
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro ao salvar anamnese. Message:'{0}'", e.Message);

                response.Sucess = false;
                response.Erro = "Erro ao salvar anamnese. Message: " + e.Message;
                return response;
            }
        }

        /// <summary>
        /// Retorna a anamnese pelo UserId (mantendo a semântica que você já usava).
        /// </summary>
        public Anamnese GetById(Guid userId)
        {
            return _context
                .Anamneses.AsNoTracking()
                .FirstOrDefault(x => x.UserId == userId);
        }

        /// <summary>
        /// Atualiza a anamnese existente de um usuário.
        /// </summary>
        public async Task<CriarAnamnese.Response> Update(CriarAnamnese.Request DTO)
        {
            var response = new CriarAnamnese.Response();

            try
            {
                var anamnese = await _context.Anamneses
                    .FirstOrDefaultAsync(x => x.UserId == DTO.UserId);

                if (anamnese == null)
                {
                    response.Sucess = false;
                    response.Erro = "Anamnese não encontrada para este usuário.";
                    return response;
                }

                // Atualiza campos
                _mapper.Map(DTO, anamnese);
                _context.Anamneses.Update(anamnese);

                var linhas = await _context.SaveChangesAsync();
                if (linhas <= 0)
                {
                    response.Sucess = false;
                    response.Erro = "Nenhuma linha afetada ao atualizar anamnese.";
                    return response;
                }

                response.Sucess = true;
                response.Id = anamnese.Id;
                response.Mensagem = "Anamnese atualizada com sucesso.";
                return response;
            }
            catch (Exception e)
            {
                response.Sucess = false;
                response.Erro = "Erro ao fazer update de anamnese. Message: " + e.Message;
                return response;
            }
        }
    }
}
