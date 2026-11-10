using System.Linq;
using Spark.Domain.Configuracao;
using Xunit;

namespace Spark.Tests
{
    public class ResolvedorPedidoTests
    {
        private static ConfigConsultas ConfigAlterada()
        {
            var c = ConfigConsultas.Padrao();
            c.Tipos.Single(t => t.Tipo == "bate_papo").Creditos = 80;
            c.Pacotes.Single(p => p.Id == "chat-3").Preco = 239.90m;
            return c;
        }

        [Fact]
        public void PacoteId_UsaPrecoECreditosDaConfiguracao()
        {
            var (pedido, erro) = ResolvedorPedido.Resolver("chat-3", 180, 196.90m, "qualquer", ConfigAlterada());
            Assert.Null(erro);
            Assert.Equal(239.90m, pedido.PrecoTotal);
            Assert.Equal(240, pedido.Creditos);
            Assert.Equal("3 consultas por bate-papo", pedido.Titulo);
        }

        [Fact]
        public void AppAntigo_SemPacoteId_MapeiaPelosCreditos()
        {
            // app 1.3.9 manda creditos=180 e o preço antigo; vale o pacote chat-3 atual
            var (pedido, erro) = ResolvedorPedido.Resolver(null, 180, 196.90m, "3 consultas por bate-papo", ConfigAlterada());
            Assert.Null(erro);
            Assert.Equal(239.90m, pedido.PrecoTotal);
            Assert.Equal(240, pedido.Creditos);
        }

        [Theory]
        [InlineData("nao-existe")]
        [InlineData("chat-5")] // inativo
        public void PacoteIdInexistenteOuInativo_Erro(string id)
        {
            var c = ConfigConsultas.Padrao();
            c.Pacotes.Single(p => p.Id == "chat-5").Ativo = false;
            var (pedido, erro) = ResolvedorPedido.Resolver(id, 0, 0, null, c);
            Assert.Null(pedido);
            Assert.Equal("Pacote indisponível.", erro);
        }

        [Fact]
        public void AppAntigo_PacoteMapeadoInativo_UsaRegraAntiga()
        {
            var c = ConfigConsultas.Padrao();
            c.Pacotes.Single(p => p.Id == "chat-3").Ativo = false;
            var (pedido, erro) = ResolvedorPedido.Resolver(null, 180, 196.90m, "3 consultas", c);
            Assert.Null(erro);
            Assert.Equal(196.90m, pedido.PrecoTotal);
            Assert.Equal(180, pedido.Creditos);
        }

        [Fact]
        public void CreditosForaDaTabela_UsaValoresDoClienteComPiso()
        {
            var (ok, erroOk) = ResolvedorPedido.Resolver(null, 1, 1m, "Teste de pagamento", ConfigConsultas.Padrao());
            Assert.Null(erroOk);
            Assert.Equal(1, ok.Creditos);

            var (_, erro) = ResolvedorPedido.Resolver(null, 1000, 100m, "barato", ConfigConsultas.Padrao());
            Assert.Equal("Valor do pedido incompatível com a quantidade de créditos.", erro);
        }

        [Theory]
        [InlineData("0.5")]
        [InlineData("1.5")]
        [InlineData("3000000000")]
        public void CreditosFracionariosOuForaDoLimite_Erro(string creditos)
        {
            var (pedido, erro) = ResolvedorPedido.Resolver(null, decimal.Parse(creditos, System.Globalization.CultureInfo.InvariantCulture), 1000m, "x", ConfigConsultas.Padrao());
            Assert.Equal("Créditos inválidos.", erro);
            Assert.Null(pedido);
        }

        [Fact]
        public void SemPacoteESemCreditos_Erro()
        {
            var (_, erro) = ResolvedorPedido.Resolver(null, 0, 10m, null, ConfigConsultas.Padrao());
            Assert.Equal("UserId e Creditos são obrigatórios.", erro);
        }
    }
}
