using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands;
using Spark.Domain.Entities;


namespace Spark.Domain.Repositories
{
    public interface ICreditosRepository
    {
        Task<bool> Create(CriarImagem.Request atividade);
        Imagem GetById(Guid id);
        void Delete(DeletarImagem.Request despesa);
    }
}