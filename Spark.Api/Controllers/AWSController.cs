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
using System.Data.SqlTypes;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Spark.Api.Controllers.FaceInsta
{
    [ApiController]
    //[Authorize(Roles = "840d02b2-2607-46e3-bbfc-7f454de0aeec")]
    [Route("aws")]
    public class AwsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AwsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("/{id}/{idapontamento}")]
        [HttpPost]
        ///get
        public GenericCommandResult PostAwsS3([FromRoute] Guid id,[FromRoute] Guid idapontamento, IFormFile file, [FromServices] ImagemHandler handler)
        {
            CriarImagem.Request command = new CriarImagem.Request { AtividadeId = id, ApontamentoId = idapontamento, ChaveAws = "", Nome = "" };

            return (GenericCommandResult)handler.Handle(command, file);
        }

        [Route("/{nomeArquivo}")]   
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

        [Route("base64/{nomeArquivo}")]
        [HttpGet]
        public async Task<IActionResult> GetImagemBase64([FromRoute] string nomeArquivo)
        {
            try
            {
                var awsKeyID = _configuration["CongifAws:KeyID"];
                var awsKeySecret = _configuration["CongifAws:SecretKey"];
                var bucketName = _configuration["CongifAws:Bucket"];
                var awsCredentials = new BasicAWSCredentials(awsKeyID, awsKeySecret);

                var config = new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.USEast1
                };
                var client = new AmazonS3Client(awsCredentials, config);

                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = nomeArquivo
                };

                using var response = await client.GetObjectAsync(request);
                using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                var contentType = response.Headers["Content-Type"] ?? "image/jpeg"; // default
                var base64 = Convert.ToBase64String(bytes);
                var dataUrl = $"data:{contentType};base64,{base64}";

                return Ok(new { base64 = dataUrl });
            }
            catch (AmazonS3Exception ex)
            {
                return StatusCode(500, $"Erro no S3: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

    }
}