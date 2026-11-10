using System.Threading.Tasks;
using Moq;
using Spark.Api.Controllers.Usuarios;
using Spark.Domain.Commands.Usuario;
using Spark.Domain.Handlers;
using Spark.Domain.Repositories;
using Xunit;

namespace Spark.Tests
{
    public class UsuarioCadastroTests
    {
        private static CriarUsuario.Request Pedido(string perfil) => new CriarUsuario.Request
        {
            Nome = "Fulano", Email = "fulano@example.com", Senha = "Senha123!", Cpf = "00000000000", Perfil = perfil, Imagem = ""
        };

        [Theory]
        [InlineData("MEDICO")]
        [InlineData("medico")]
        [InlineData(null)]
        public async Task CadastroPublico_DeNaoPaciente_Recusado(string perfil)
        {
            var repo = new Mock<IUsuarioRepository>();
            var r = await new UsuarioController().Create(Pedido(perfil), new UsuarioHandler(repo.Object));

            Assert.False(r.Success);
            Assert.Equal("Cadastro de médico é feito por um médico já cadastrado.", r.Message);
            repo.Verify(x => x.Create(It.IsAny<CriarUsuario.Request>()), Times.Never);
        }

        [Fact]
        public async Task CadastroPublico_DePaciente_Segue()
        {
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(x => x.Create(It.IsAny<CriarUsuario.Request>())).ReturnsAsync(new CriarUsuario.Response { Sucess = true, Mensagem = "ok" });

            var r = await new UsuarioController().Create(Pedido("PACIENTE"), new UsuarioHandler(repo.Object));

            Assert.True(r.Success);
            repo.Verify(x => x.Create(It.IsAny<CriarUsuario.Request>()), Times.Once);
        }
    }
}
