using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Spark.Domain.Commands;

namespace Spark.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "acb3830a-402b-4865-9f70-7b28d39f66ad,efe703c2-2076-4d5b-aa04-aaee90953e49,e0e938ef-5ca3-48bc-b5a5-30a0fd0b64ca")]
    [Route("chat")]
    public class ChatFileUploadController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private const long MaxFileSize = 25 * 1024 * 1024; // 25MB

        private static readonly string[] AllowedExtensions = new[]
        {
            // Imagens
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".heic",
            // Videos
            ".mp4", ".mov", ".avi", ".mkv", ".webm", ".3gp",
            // Documentos
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt"
        };

        public ChatFileUploadController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Upload de arquivo para o chat da consulta.
        /// Aceita imagens, videos e documentos (max 25MB).
        /// Retorna a URL publica do arquivo no S3.
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(26_214_400)] // ~25MB + overhead
        public async Task<IActionResult> UploadChatFile(IFormFile file, [FromForm] string fileName, [FromForm] string mimeType)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "Nenhum arquivo enviado." });

            if (file.Length > MaxFileSize)
                return BadRequest(new { Message = "Arquivo muito grande. Tamanho maximo: 25MB." });

            var originalName = fileName ?? file.FileName;
            var extension = Path.GetExtension(originalName)?.ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || Array.IndexOf(AllowedExtensions, extension) < 0)
                return BadRequest(new { Message = $"Tipo de arquivo nao permitido: {extension}" });

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

                using var client = new AmazonS3Client(awsCredentials, config);

                // Gerar chave unica no S3: chat-files/{ano}/{mes}/{guid}{ext}
                var now = DateTime.UtcNow;
                var s3Key = $"chat-files/{now:yyyy}/{now:MM}/{Guid.NewGuid()}{extension}";

                using var stream = file.OpenReadStream();

                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = s3Key,
                    InputStream = stream,
                    ContentType = mimeType ?? file.ContentType ?? "application/octet-stream",
                    AutoCloseStream = false,
                };

                var response = await client.PutObjectAsync(putRequest);

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    return StatusCode(500, new { Message = "Falha ao enviar arquivo para o S3." });

                // Gerar URL pre-assinada com validade de 24h
                var preSignedRequest = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = s3Key,
                    Expires = DateTime.UtcNow.AddHours(24),
                };

                var fileUrl = client.GetPreSignedURL(preSignedRequest);

                var result = new ChatFileUploadResponse
                {
                    Url = fileUrl,
                    FileName = originalName,
                    FileSize = file.Length,
                    MimeType = mimeType ?? file.ContentType ?? "application/octet-stream",
                };

                return Ok(result);
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"[CHAT-UPLOAD] Erro S3: {ex.Message}");
                return StatusCode(500, new { Message = $"Erro no armazenamento: {ex.Message}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHAT-UPLOAD] Erro: {ex.Message}");
                return StatusCode(500, new { Message = $"Erro interno: {ex.Message}" });
            }
        }
    }
}
