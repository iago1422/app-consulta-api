using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spark.Domain.Commands;

namespace Spark.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "acb3830a-402b-4865-9f70-7b28d39f66ad,efe703c2-2076-4d5b-aa04-aaee90953e49,e0e938ef-5ca3-48bc-b5a5-30a0fd0b64ca")]
    [Route("chat")]
    public class ChatFileUploadController : ControllerBase
    {
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        private static readonly string[] AllowedExtensions = new[]
        {
            // Imagens
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".heic",
            // Videos
            ".mp4", ".mov", ".avi", ".mkv", ".webm", ".3gp",
            // Documentos
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt"
        };

        /// <summary>
        /// Upload de arquivo para o chat da consulta.
        /// Recebe o arquivo, converte para base64 em memoria e retorna a data URL.
        /// Nenhum arquivo e persistido - apenas transito em memoria.
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(11_534_336)] // ~11MB (10MB + overhead do multipart)
        public async Task<IActionResult> UploadChatFile(IFormFile file, [FromForm] string fileName, [FromForm] string mimeType)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "Nenhum arquivo enviado." });

            if (file.Length > MaxFileSize)
                return BadRequest(new { Message = "Arquivo muito grande. Tamanho maximo: 10MB." });

            var originalName = fileName ?? file.FileName;
            var extension = Path.GetExtension(originalName)?.ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || Array.IndexOf(AllowedExtensions, extension) < 0)
                return BadRequest(new { Message = $"Tipo de arquivo nao permitido: {extension}" });

            try
            {
                var contentType = mimeType ?? file.ContentType ?? "application/octet-stream";

                // Ler o arquivo em memoria e converter para base64
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                var base64String = Convert.ToBase64String(fileBytes);
                var dataUrl = $"data:{contentType};base64,{base64String}";

                var result = new ChatFileUploadResponse
                {
                    Url = dataUrl,
                    FileName = originalName,
                    FileSize = file.Length,
                    MimeType = contentType,
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHAT-UPLOAD] Erro: {ex.Message}");
                return StatusCode(500, new { Message = $"Erro ao processar arquivo: {ex.Message}" });
            }
        }
    }
}
