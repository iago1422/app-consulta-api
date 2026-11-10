using System.Linq;
using Spark.Domain.Configuracao;
using Xunit;

namespace Spark.Tests
{
    public class ConfigConsultasTests
    {
        [Fact]
        public void Padrao_TemValoresAtuaisEValida()
        {
            var c = ConfigConsultas.Padrao();
            Assert.Empty(c.Validar());
            Assert.Equal(60, c.CreditosDe("bate_papo"));
            Assert.Equal(70, c.CreditosDe("audio_video"));
            Assert.Equal(180, c.CreditosDoPacote(c.Pacotes.Single(p => p.Id == "chat-3")));
            Assert.Equal(72.90m, c.Pacotes.Single(p => p.Id == "chat-1").Preco);
        }

        [Fact]
        public void CreditosDoPacote_AcompanhaCreditosDoTipo()
        {
            var c = ConfigConsultas.Padrao();
            c.Tipos.Single(t => t.Tipo == "bate_papo").Creditos = 80;
            Assert.Equal(240, c.CreditosDoPacote(c.Pacotes.Single(p => p.Id == "chat-3")));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1001)]
        public void Creditos_ForaDoLimite_Invalido(int creditos)
        {
            var c = ConfigConsultas.Padrao();
            c.Tipos[0].Creditos = creditos;
            Assert.Contains(c.Validar(), e => e.Contains("entre 1 e 1000"));
        }

        [Fact]
        public void TipoAusente_Invalido()
        {
            var c = ConfigConsultas.Padrao();
            c.Tipos.RemoveAll(t => t.Tipo == "audio_video");
            Assert.Contains(c.Validar(), e => e.Contains("audio_video"));
        }

        [Fact]
        public void PacoteRepetido_Invalido()
        {
            var c = ConfigConsultas.Padrao();
            c.Pacotes.Add(new PacoteConfig { Id = "chat-1", Tipo = "bate_papo", Consultas = 1, Preco = 80m });
            Assert.Contains(c.Validar(), e => e.Contains("repetido"));
        }

        [Fact]
        public void PrecoAbaixoDoPiso_Invalido_MasInativoPassa()
        {
            var c = ConfigConsultas.Padrao();
            var p = c.Pacotes.Single(x => x.Id == "chat-1");
            p.Preco = 50m; // 60 créditos × 0,95 = 57
            Assert.Contains(c.Validar(), e => e.Contains("abaixo do mínimo") && e.Contains("57,00"));

            p.Ativo = false;
            Assert.DoesNotContain(c.Validar(), e => e.Contains("abaixo do mínimo"));
        }

        [Fact]
        public void PrecoComTresCasas_Invalido()
        {
            var c = ConfigConsultas.Padrao();
            c.Pacotes[0].Preco = 72.905m;
            Assert.Contains(c.Validar(), e => e.Contains("2 casas"));
        }

        [Fact]
        public void TipoSemPacoteAtivo_Invalido()
        {
            var c = ConfigConsultas.Padrao();
            c.Pacotes.Where(p => p.Tipo == "audio_video").ToList().ForEach(p => p.Ativo = false);
            Assert.Contains(c.Validar(), e => e.Contains("pelo menos um pacote ativo"));
        }
    }
}
