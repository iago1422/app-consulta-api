using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spark.Domain.Commands;
using Spark.Domain.Entities;
using Spark.Domain.Handlers;
using Spark.Domain.Handlers.Imagem;
using Spark.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace Spark.Api.Controllers
{
    [ApiController]
    //[Authorize(Roles = "d7ccf34b-b722-4735-a1c2-a95c2b472eb1")]
    [Route("imagens")]
    public class ImagemController : ControllerBase
    {
        private readonly IImagemRepository _repository; 
        private readonly IConfiguration _configuration;

        public ImagemController(IImagemRepository repository, IConfiguration configuration)  // Construtor para injetar o repositório
        {
            _repository = repository;
            _configuration = configuration;
        }      
     
        [Route("get-by-id/{idatividade}")]
        [HttpGet]
        ///get
        public async Task<IActionResult> GetById([FromServices] IImagemRepository repository, [FromRoute] Guid idatividade)
        {
            return Ok(new { Data = repository.GetById(idatividade) });
        }

        [Route("upload-imagem-usuario")]
        [HttpPost]
        public async Task<IActionResult> UploadImagem(IFormFile file, [FromServices] IImagemRepository repository)
        {
            var command = new CriarImagemUsuario.Request { ChaveAws = "", Nome = "" };

            // Chama o repository para fazer o upload da imagem e gerar o link
            var preSignedUrl = await repository.UploadImageToS3(command, file);

            if (string.IsNullOrEmpty(preSignedUrl))
            {
                return StatusCode(500, "Erro ao fazer upload da imagem para o S3.");
            }

            return Ok(new { Message = "Upload bem-sucedido", chaveAWS = preSignedUrl });
        }

        [Route("/get-imagem-usuario/{nomeArquivo}")]
        [HttpGet]
        ///post
        public async Task<string> GetAwsS3([FromRoute] string nomeArquivo)
        {
            try
            {
                var awsKeyID = _configuration["CongifAws:KeyID"];
                var awsKeySecret = _configuration["CongifAws:SecretKey"];
                var BucketName = _configuration["CongifAws:Bucket"];
                var awsCredentials = new BasicAWSCredentials(awsKeyID, awsKeySecret);

                var config = new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.USEast1
                };
                var client = new AmazonS3Client(awsCredentials, config);
                var bucketExist = await AmazonS3Util.DoesS3BucketExistAsync(client, BucketName);

                if (bucketExist)
                {

                    GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                    {
                        BucketName = BucketName,
                        Key = nomeArquivo,
                        Expires = DateTime.UtcNow.AddHours(1),
                    };

                    var urlString = client.GetPreSignedURL(request1);

                    return urlString;
                }
                // Save object to local file
                //await response.WriteResponseStreamToFileAsync($"{filePath}\\{objectName}", true, CancellationToken.None);
                //return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object"
                        , e.Message);
                return "";

            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object"
                    , e.Message);
                return "";
            }

            return "";
        }
    }
}