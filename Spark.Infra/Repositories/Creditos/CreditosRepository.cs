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
                // 1. Verifica se o usuário já tem conta de crédito
                var conta = await _context.ContasCredito
                    .FirstOrDefaultAsync(x => x.UserId == DTO.UserId);

                if (conta == null)
                {
                    // 2. Não existe -> cria uma nova conta
                    conta = _mapper.Map<ContaCredito>(DTO);

                    // Garante que o saldo inicial venha do DTO
                    // Ex: conta.Saldo = DTO.Saldo;
                    await _context.ContasCredito.AddAsync(conta);
                }
                else
                {
                    // 3. Já existe -> soma o valor ao saldo
                    conta.Saldo += DTO.Saldo; // ou DTO.Valor, dependendo do teu modelo

                    _context.ContasCredito.Update(conta);
                }

                // 4. Cria a movimentação de crédito (COMPRA)
                var movimentacao = new MovimentacaoCredito
                {
                    // Ajusta esses campos conforme tua entidade:
                    UserId = conta.UserId,                      
                    Referencia = string.Empty, // se tiver no DTO
                    Tipo = "COMPRA",
                    Valor = DTO.Saldo                    
                };

                await _context.MovimentacoesCredito.AddAsync(movimentacao);

                // 5. Salva tudo de uma vez
                var linhas = await _context.SaveChangesAsync();

                if (linhas <= 0)
                {
                    response.Sucess = false;
                    response.Erro = "Nenhuma linha afetada ao salvar créditos.";
                    return response;
                }

                response.Sucess = true;
                response.Id = conta.Id;
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

        public async Task<CriarCreditos.Response> Update(CriarCreditos.Request DTO)
        {
            var response = new CriarCreditos.Response();

            try
            {
                // 1. Buscar a conta existente
                var conta = await _context.ContasCredito
                    .FirstOrDefaultAsync(x => x.UserId == DTO.UserId);

                if (conta == null)
                {
                    response.Sucess = false;
                    response.Erro = "Conta de crédito não encontrada.";
                    return response;
                }

                // 2. Subtrair o valor recebido do saldo existente
                // Ex: conta.Saldo atual = 100
                //     DTO.Saldo = 30 -> novo saldo = 70
                conta.Saldo -= DTO.Saldo;

                if (conta.Saldo < 0)
                {
                    response.Sucess = false;
                    response.Erro = "Saldo insuficiente para realizar esta operação.";
                    return response;
                }

                _context.ContasCredito.Update(conta);

                // 3. Registrar movimentação (USO)
                var movimentacao = new MovimentacaoCredito
                {
                    UserId = conta.UserId,
                    Tipo = "USO",  // Ex.: "USO"
                    Valor = DTO.Saldo,            // valor movimentado (não o saldo total!)
                    Referencia = ""
                };

                await _context.MovimentacoesCredito.AddAsync(movimentacao);

                // 4. Salvar tudo
                await _context.SaveChangesAsync();

                response.Sucess = true;
                response.Id = conta.Id;
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
