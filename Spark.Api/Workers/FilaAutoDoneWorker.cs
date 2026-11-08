using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spark.Api.Hubs;
using Spark.Domain.Entities;
using Spark.Domain.Infra.Contexts;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Spark.Infra.Workers
{
    public class FilaAutoDoneWorker : BackgroundService
    {
        // Prazos de expiração da fila (decisão de produto, ajuste aqui):
        // LEFT: paciente já saiu; mantém 1h para o médico ainda conseguir finalizar.
        public static readonly TimeSpan PrazoLeft = TimeSpan.FromHours(1);
        // CALLED: consulta em andamento; 3h cobre a consulta mais longa.
        public static readonly TimeSpan PrazoCalled = TimeSpan.FromHours(3);
        // WAITING: paciente esquecido na fila (app fechado) depois de 6h.
        public static readonly TimeSpan PrazoWaiting = TimeSpan.FromHours(6);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FilaAutoDoneWorker> _logger;
        private readonly IHubContext<ConsultHub> _hub;

        public FilaAutoDoneWorker(IServiceScopeFactory scopeFactory, ILogger<FilaAutoDoneWorker> logger, IHubContext<ConsultHub> hub)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _hub = hub;
        }

        // Registros que devem virar DONE (público para teste unitário)
        public static Expression<Func<FilaAtendimento, bool>> Expirados(DateTime now)
        {
            var limiteLeft = now - PrazoLeft;
            var limiteCalled = now - PrazoCalled;
            var limiteWaiting = now - PrazoWaiting;

            return x =>
                (x.Status == "LEFT" && (x.UpdatedAt ?? x.CreatedAt) <= limiteLeft) ||
                (x.Status == "CALLED" && (x.CalledAt ?? x.CreatedAt) <= limiteCalled) ||
                (x.Status == "WAITING" && x.CreatedAt <= limiteWaiting);
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

                    var now = DateTime.UtcNow;

                    var pendentes = await context.FilaAtendimento
                        .Where(Expirados(now))
                        .ToListAsync(stoppingToken);

                    if (pendentes.Count > 0)
                    {
                        foreach (var item in pendentes)
                        {
                            item.Status = "DONE";
                            item.UpdatedAt = now;
                        }

                        await context.SaveChangesAsync(stoppingToken);

                        foreach (var item in pendentes)
                            await ConsultHub.NotificarStatusFila(_hub, item.Id, "DONE");

                        _logger.LogInformation("FilaAutoDoneWorker: {Count} registros expirados.", pendentes.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FilaAutoDoneWorker falhou ao processar.");
                }

                // roda de 1 em 1 minuto
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("FilaAutoDoneWorker finalizado.");
        }
    }
}
