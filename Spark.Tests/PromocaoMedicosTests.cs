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

        [Fact]
        public void Ids_ExtraiGuidsEIgnoraEmails()
        {
            var lista = "894139c9-8628-497b-82b0-6afe5f61a56e, ana@example.com; 894139C9-8628-497B-82B0-6AFE5F61A56E lixo";
            Assert.Equal(new[] { System.Guid.Parse("894139c9-8628-497b-82b0-6afe5f61a56e") }, PromocaoMedicos.Ids(lista));
            Assert.Equal(new[] { "ANA@EXAMPLE.COM" }, PromocaoMedicos.Emails(lista));
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
