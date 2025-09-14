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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Spark.Domain.Commands;
using Spark.Domain.Entities;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;
using static System.Net.Mime.MediaTypeNames;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Spark.Domain.Infra.Repositories
{
    public class ImagemRepository : IImagemRepository
    {
        private readonly DataContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public ImagemRepository(DataContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }
        public async Task<bool> Create(CriarImagem.Request DTO, IFormFile File)
        {
            try
            {
                var awsKeyID = _configuration["CongifAws:KeyID"];
                var awsKeySecret = _configuration["CongifAws:SecretKey"];
                var BucketName = _configuration["CongifAws:Bucket"];

                //salva imagem no banco
                DTO.ChaveAws = Guid.NewGuid().ToString();
                DTO.Nome = DateTime.Now.ToString();

                var entidade = _mapper.Map<Imagem>(DTO);
                _context.Imagens.Add(entidade);
                _context.SaveChanges();

                //salva imagem na AWS S3
                var awsCredentials = new BasicAWSCredentials(awsKeyID, awsKeySecret);

                var arquivoEmMemoria = new MemoryStream();
                File.CopyTo(arquivoEmMemoria);

                var config = new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.USEast1
                };
                var client = new AmazonS3Client(awsCredentials, config);
                var bucketExist = await AmazonS3Util.DoesS3BucketExistAsync(client, BucketName);

                if (!bucketExist)
                {
                    return false;
                }

                var fileTransferUtilit = new TransferUtility(client);
                var Transfer = new TransferUtilityUploadRequest
                {
                    InputStream = arquivoEmMemoria,
                    Key = DTO.ChaveAws,
                    BucketName = BucketName,
                    ContentType = File.ContentType
                };

                // Adiciona o cabeçalho Cache-Control usando Metadata
                Transfer.Metadata.Add("Cache-Control", "max-age=31536000, public");

                await fileTransferUtilit.UploadAsync(Transfer);


                return true;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object"
                        , e.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object"
                    , e.Message);
                return false;
            }
        }
        public Imagem GetById(Guid id)
        {
            return _context
                .Imagens.AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
        }    
     

        public void Delete(DeletarImagem.Request imagem)
        {
            var imagemResponse = _context.Imagens.FirstOrDefault(x => x.Id == imagem.Id);
            _context.Imagens.Remove(imagemResponse);
            _context.SaveChanges();
        }

      
        public async Task<string> UploadImageToS3(CriarImagemUsuario.Request DTO, IFormFile File)
        {
            try
            {
                var awsKeyID = _configuration["CongifAws:KeyID"];
                var awsKeySecret = _configuration["CongifAws:SecretKey"];
                var BucketName = _configuration["CongifAws:Bucket"];

                DTO.ChaveAws = Guid.NewGuid().ToString();
                DTO.Nome = DateTime.Now.ToString();

                //var entidade = _mapper.Map<ImagemUsuario>(DTO);
                //_context.ImagensUsuario.Add(entidade);
                //_context.SaveChanges();

                var awsCredentials = new BasicAWSCredentials(awsKeyID, awsKeySecret);
                using var arquivoEmMemoria = new MemoryStream();
                File.CopyTo(arquivoEmMemoria);

                var config = new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.USEast1
                };
                var client = new AmazonS3Client(awsCredentials, config);

                var bucketExist = await AmazonS3Util.DoesS3BucketExistAsync(client, BucketName);
                if (!bucketExist)
                {
                    return null;
                }

                var fileTransferUtilit = new TransferUtility(client);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = arquivoEmMemoria,
                    Key = DTO.ChaveAws,
                    BucketName = BucketName,
                    ContentType = File.ContentType
                };

                await fileTransferUtilit.UploadAsync(uploadRequest);

                return uploadRequest.Key;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine($"Erro ao tentar fazer upload para o S3: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro desconhecido: {e.Message}");
                return null;
            }
        }

    }
}
