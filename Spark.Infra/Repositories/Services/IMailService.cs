using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Spark.Infra.Repositories.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(string fromEmail, string toEmail, string plainTextContent, string htmlContent);
    }
    public class SendGridEmailService : IMailService
    {
        private readonly IConfiguration _configuration;

        public SendGridEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task SendEmailAsync(string fromEmail, string toEmail, string plainTextContent, string htmlContent)
        {
            var subject = "Convite de Projeto";
            var apiKey = _configuration["SendGridAPIKey"];
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(fromEmail);
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            // Desativando o rastreamento de cliques
            msg.TrackingSettings = new TrackingSettings
            {
                ClickTracking = new ClickTracking
                {
                    Enable = false,     // Desativa rastreamento de links no HTML
                    EnableText = false  // Desativa rastreamento de links no texto simples
                }
            };

            try
            {
                var response = await client.SendEmailAsync(msg);
                Console.WriteLine($"Status Code: {response.StatusCode}");
                var responseBody = await response.Body.ReadAsStringAsync();
                Console.WriteLine($"Response Body: {responseBody}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
            }
        }


    }
}
