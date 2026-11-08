using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Spark.Domain.Commands;
using Spark.Domain.Commands.Usuario;
using Spark.Domain.Entities;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;
using static System.Net.Mime.MediaTypeNames;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Spark.Domain.Infra.Repositories
{
    public class CreditosRepository : ICreditosRepository
    {
        private readonly DataContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public CreditosRepository(DataContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }
        public async Task<CriarCreditos.Response> Create(CriarCreditos.Request DTO)
        {
            var response = new CriarCreditos.Response();

            try
            {
                using var tx = await _context.Database.BeginTransactionAsync();

                // 1. Verifica se o usuário já tem conta de crédito
                var contaId = await _context.ContasCredito
                    .Where(x => x.UserId == DTO.UserId)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync();

                if (contaId == null)
                {
                    // 2. Não existe -> cria uma nova conta com o saldo do DTO
                    var conta = _mapper.Map<ContaCredito>(DTO);
                    await _context.ContasCredito.AddAsync(conta);
                    contaId = conta.Id;
                }
                else
                {
                    // 3. Já existe -> soma no banco (atômico, sem lost update com um débito concorrente)
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE ""ContasCredito"" SET ""Saldo"" = ""Saldo"" + {DTO.Saldo}, ""UpdatedAt"" = {DateTime.UtcNow} WHERE ""UserId"" = {DTO.UserId}");
                }

                // 4. Cria a movimentação de crédito (COMPRA)
                await _context.MovimentacoesCredito.AddAsync(new MovimentacaoCredito
                {
                    UserId = DTO.UserId,
                    Referencia = DTO.Referencia,
                    Tipo = "COMPRA",
                    Valor = DTO.Saldo
                });

                // 5. Salva tudo de uma vez
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                response.Sucess = true;
                response.Id = contaId.Value;
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro ao criar créditos. Message:'{0}'", e.Message);

                response.Sucess = false;
                response.Erro = "Erro ao criar créditos. Message: " + e.Message;
                return response;
            }
        }
        public ContaCredito GetById(Guid id)
        {
            return _context
                .ContasCredito.AsNoTracking()
                .FirstOrDefault(x => x.UserId == id);
        }

        public async Task<bool> ExisteMovimentacao(Guid userId, string tipo, string referencia)
        {
            return await _context.MovimentacoesCredito
                .AnyAsync(m => m.UserId == userId && m.Tipo == tipo && m.Referencia == referencia);
        }

        public async Task<CriarCreditos.Response> Update(CriarCreditos.Request DTO)
        {
            var response = new CriarCreditos.Response();

            try
            {
                using var tx = await _context.Database.BeginTransactionAsync();

                // 1. Buscar a conta existente
                var contaId = await _context.ContasCredito
                    .Where(x => x.UserId == DTO.UserId)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync();

                if (contaId == null)
                {
                    response.Sucess = false;
                    response.Erro = "Conta de crédito não encontrada.";
                    return response;
                }

                response.Id = contaId.Value;

                // 2. Idempotência: mesmo débito (referencia = id da fila) já feito -> sucesso sem debitar de novo
                if (!string.IsNullOrWhiteSpace(DTO.Referencia) && await ExisteMovimentacao(DTO.UserId, "USO", DTO.Referencia))
                {
                    response.Sucess = true;
                    response.Mensagem = "Débito já realizado anteriormente.";
                    return response;
                }

                // 3. Subtrair no banco só se houver saldo (atômico, sem lost update com crédito concorrente do webhook)
                var linhas = await _context.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE ""ContasCredito"" SET ""Saldo"" = ""Saldo"" - {DTO.Saldo}, ""UpdatedAt"" = {DateTime.UtcNow} WHERE ""UserId"" = {DTO.UserId} AND ""Saldo"" >= {DTO.Saldo}");

                if (linhas == 0)
                {
                    response.Sucess = false;
                    response.Erro = "Saldo insuficiente para realizar esta operação.";
                    return response;
                }

                // 4. Registrar movimentação (USO)
                await _context.MovimentacoesCredito.AddAsync(new MovimentacaoCredito
                {
                    UserId = DTO.UserId,
                    Tipo = "USO",
                    Valor = DTO.Saldo,            // valor movimentado (não o saldo total!)
                    Referencia = DTO.Referencia
                });

                // 5. Salvar tudo
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                response.Sucess = true;
                return response;
            }
            catch (DbUpdateException e) when (!string.IsNullOrWhiteSpace(DTO.Referencia) && e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // retry concorrente com a mesma referencia: o outro já debitou, este foi revertido (rollback)
                response.Sucess = true;
                response.Mensagem = "Débito já realizado anteriormente.";
                return response;
            }
            catch (Exception e)
            {
                response.Sucess = false;
                response.Erro = "Erro ao fazer update de créditos. Message: " + e.Message;
                return response;
            }
        }

    }
}
