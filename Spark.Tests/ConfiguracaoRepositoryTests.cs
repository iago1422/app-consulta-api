using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spark.Api.Services;
using Spark.Domain.Configuracao;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Infra.Repositories;
using Xunit;

namespace Spark.Tests
{
    public class ConfiguracaoRepositoryTests
    {
        private static TestDbContext NovoContexto() =>
            new TestDbContext(new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        [Fact]
        public async Task SemLinhaSalva_DevolvePadrao()
        {
            var salva = await new ConfiguracaoRepository(NovoContexto()).ObterConsultas();
            Assert.Equal(60, salva.Config.CreditosDe("bate_papo"));
            Assert.Null(salva.AtualizadoEm);
            Assert.Null(salva.AtualizadoPor);
        }

        [Fact]
        public async Task Salvar_DepoisObter_DevolveValoresEAutor()
        {
            var ctx = NovoContexto();
            var repo = new ConfiguracaoRepository(ctx);
            var autor = Guid.NewGuid();
            var config = ConfigConsultas.Padrao();
            config.Tipos.Single(t => t.Tipo == "bate_papo").Creditos = 80;
            config.Pacotes.Single(p => p.Id == "chat-1").Preco = 79.90m;

            await repo.SalvarConsultas(config, autor);
            await repo.SalvarConsultas(config, autor); // segunda gravação atualiza a mesma linha

            var salva = await new ConfiguracaoRepository(ctx).ObterConsultas();
            Assert.Equal(80, salva.Config.CreditosDe("bate_papo"));
            Assert.Equal(79.90m, salva.Config.Pacotes.Single(p => p.Id == "chat-1").Preco);
            Assert.Equal(autor, salva.AtualizadoPor);
            Assert.NotNull(salva.AtualizadoEm);
            Assert.Equal(1, await ctx.Configuracoes.CountAsync());
        }

        [Fact]
        public void UsuarioId_LeNameIdentifier()
        {
            var id = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()) }, "jwt"));
            Assert.Equal(id, user.UsuarioId());
            Assert.Null(new ClaimsPrincipal(new ClaimsIdentity()).UsuarioId());
        }
    }
}
