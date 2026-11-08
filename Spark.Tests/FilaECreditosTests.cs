using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Spark.Api.Controllers;
using Spark.Domain.Commands;
using Spark.Domain.Entities;
using Spark.Domain.Repositories;
using Spark.Infra.Workers;
using Xunit;

namespace Spark.Tests
{
    public class FilaAutoDoneWorkerTests
    {
        private static readonly DateTime Agora = new DateTime(2026, 1, 1, 12, 0, 0);

        [Theory]
        [InlineData("WAITING", 5, null, null, false)]    // acabou de entrar na fila
        [InlineData("WAITING", 7 * 60, null, null, true)] // esquecido na fila
        [InlineData("CALLED", 5 * 60, 30, null, false)]   // consulta em andamento
        [InlineData("CALLED", 5 * 60, 4 * 60, null, true)]
        [InlineData("LEFT", 10, 5, 2, false)]             // médico ainda pode finalizar
        [InlineData("LEFT", 3 * 60, null, 2 * 60, true)]
        [InlineData("DONE", 30 * 60, null, null, false)]
        public void Expirados_AplicaPrazoPorStatus(string status, int criadoHaMin, int? chamadoHaMin, int? atualizadoHaMin, bool esperado)
        {
            var item = new FilaAtendimento
            {
                Status = status,
                CreatedAt = Agora.AddMinutes(-criadoHaMin),
                CalledAt = chamadoHaMin.HasValue ? Agora.AddMinutes(-chamadoHaMin.Value) : null,
                UpdatedAt = atualizadoHaMin.HasValue ? Agora.AddMinutes(-atualizadoHaMin.Value) : null,
            };

            var expirados = new List<FilaAtendimento> { item }.AsQueryable()
                .Where(FilaAutoDoneWorker.Expirados(Agora))
                .ToList();

            Assert.Equal(esperado, expirados.Count == 1);
        }
    }

    public class CreditoControllerTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task Debito_ComSaldoNaoPositivo_Retorna400SemDebitar(double saldo)
        {
            var repo = new Mock<ICreditosRepository>();
            var controller = new CreditoController(repo.Object, null);

            var result = await controller.Update(new CriarCreditos.Request { UserId = Guid.NewGuid(), Saldo = saldo }, repo.Object);

            Assert.IsType<BadRequestObjectResult>(result);
            repo.Verify(r => r.Update(It.IsAny<CriarCreditos.Request>()), Times.Never);
        }

        [Theory]
        [InlineData(false, 409)]
        [InlineData(true, 200)]
        public async Task Debito_MapeiaResultadoDoRepositorioParaStatus(bool sucesso, int statusEsperado)
        {
            var repo = new Mock<ICreditosRepository>();
            repo.Setup(r => r.Update(It.IsAny<CriarCreditos.Request>()))
                .ReturnsAsync(new CriarCreditos.Response { Sucess = sucesso, Erro = sucesso ? null : "Saldo insuficiente para realizar esta operação." });
            var controller = new CreditoController(repo.Object, null);

            var result = await controller.Update(new CriarCreditos.Request { UserId = Guid.NewGuid(), Saldo = 60 }, repo.Object);

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(statusEsperado, objectResult.StatusCode);
        }
    }
}
