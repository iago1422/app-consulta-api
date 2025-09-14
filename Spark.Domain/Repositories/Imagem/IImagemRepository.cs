using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands;
using Spark.Domain.Entities;


namespace Spark.Domain.Repositories
{
    public interface IImagemRepository
    {
        Task<bool> Create(CriarImagem.Request atividade, IFormFile File);
        Imagem GetById(Guid id);
        void Delete(DeletarImagem.Request despesa);
        Task<string> UploadImageToS3(CriarImagemUsuario.Request atividade, IFormFile file);
    }
}