using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spark.Infra.Repositories.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(string fromEmail, string toEmail, string plainTextContent, string htmlContent);
    }

    public class ResendEmailService : IMailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ResendEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task SendEmailAsync(string fromEmail, string toEmail, string plainTextContent, string htmlContent)
        {
            var subject = "IConsulta";
            var apiKey = _configuration["ResendApiKey"];

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            var body = new
            {
                from = fromEmail,
                to = new[] { toEmail },
                subject,
                text = plainTextContent,
                html = htmlContent
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Status Code: {(int)response.StatusCode}");
                Console.WriteLine($"Response Body: {responseBody}");

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
            }
        }
    }
}