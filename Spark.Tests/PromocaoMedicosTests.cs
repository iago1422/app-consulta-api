using Spark.Domain.Infra.Contexts;
using Xunit;

namespace Spark.Tests
{
    public class PromocaoMedicosTests
    {
        [Fact]
        public void Emails_AceitaSeparadoresVariados_NormalizaEDeduplica()
        {
            var emails = PromocaoMedicos.Emails(" Dra.Ana@Example.com, joao@example.com;JOAO@example.com\nmaria@example.com ");
            Assert.Equal(new[] { "DRA.ANA@EXAMPLE.COM", "JOAO@EXAMPLE.COM", "MARIA@EXAMPLE.COM" }, emails);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("nao-e-email, outro")]
        public void Emails_SemEmailValido_ListaVazia(string lista)
        {
            Assert.Empty(PromocaoMedicos.Emails(lista));
        }
    }
}
