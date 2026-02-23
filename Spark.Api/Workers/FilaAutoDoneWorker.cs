using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spark.Domain.Infra.Contexts;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace Spark.Infra.Workers
{
    public class FilaAutoDoneWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FilaAutoDoneWorker> _logger;

        public FilaAutoDoneWorker(IServiceScopeFactory scopeFactory, ILogger<FilaAutoDoneWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FilaAutoDoneWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                    var limit = DateTime.UtcNow.AddHours(-1);

                    // Regra: LEFT + CalledAt <= agora - 1h => DONE
                    var pendentes = await context.FilaAtendimento
                        .Where(x => x.Status == "WAITING" && x.CreatedAt <= limit)
                        .ToListAsync(stoppingToken);

                    if (pendentes.Count > 0)
                    {
                        foreach (var item in pendentes)
                        {
                            item.Status = "DONE";           // ou "ATENDIDO"
                            item.UpdatedAt = DateTime.UtcNow;
                        }

                        await context.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("FilaAutoDoneWorker: {Count} registros atualizados.", pendentes.Count);
                    }
                    else { 
                        _logger.LogInformation("FilaAutoDoneWorker: {Count} registros atualizados.", pendentes.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FilaAutoDoneWorker falhou ao processar.");
                }

                // roda de 1 em 1 minuto (ajuste como quiser)
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("FilaAutoDoneWorker finalizado.");
        }
    }
}