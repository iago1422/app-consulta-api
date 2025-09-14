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
                var entidade = _mapper.Map<ContaCredito>(DTO);                

                _context.ContasCredito.Add(entidade);
                var result = _context.SaveChanges();

                await _context.ContasCredito.AddAsync(entidade);
                var linhas = await _context.SaveChangesAsync();

                if (linhas <= 0)
                {
                    response.Sucess = false;
                    response.Erro = "Nenhuma linha afetada ao salvar.";
                    return response;
                }

                response.Sucess = true;                
                response.Id = entidade.Id;                

                return response;
            }         
            catch (Exception e)
            {
                Console.WriteLine(
                    "Erro ao criar créditos. Message:'{0}'"
                    , e.Message);

                response.Sucess = false;
                response.Erro = "Erro ao criar créditos. Message: " + e.Message;
                return response;
            }
        }
        public ContaCredito GetById(Guid id)
        {
            return _context
                .ContasCredito.AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
        }

    }
}
