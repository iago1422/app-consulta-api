using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Spark.Api.Controllers;
using Spark.Api.Services;
using Spark.Domain.Configuracao;
using Spark.Domain.Repositories;
using Xunit;

namespace Spark.Tests
{
    public class AdminControllerTests
    {
        private static AdminController Controller(Mock<IConfiguracaoRepository> repo, Guid usuario)
        {
            repo.Setup(r => r.ObterConsultas()).ReturnsAsync(new ConfigConsultasSalva(ConfigConsultas.Padrao(), null, null));
            return new AdminController(null, repo.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, usuario.ToString()) }, "jwt"))
                    }
                }
            };
        }

        [Fact]
        public void Admin_NaoAceitaPerfilPaciente()
        {
            var roles = typeof(AdminController).GetCustomAttribute<AuthorizeAttribute>().Roles;
            Assert.DoesNotContain(Perfis.Paciente, roles);
            Assert.Contains(Perfis.Medico, roles);
        }

        [Fact]
        public async Task SalvarConfig_Invalida_Retorna400SemGravar()
        {
            var repo = new Mock<IConfiguracaoRepository>();
            var config = ConfigConsultas.Padrao();
            config.Tipos[0].Creditos = 0;

            var result = await Controller(repo, Guid.NewGuid()).SalvarConfig(config);

            Assert.IsType<BadRequestObjectResult>(result);
            repo.Verify(r => r.SalvarConsultas(It.IsAny<ConfigConsultas>(), It.IsAny<Guid?>()), Times.Never);
        }

        [Fact]
        public async Task SalvarConfig_Valida_GravaComAutorDoToken()
        {
            var repo = new Mock<IConfiguracaoRepository>();
            var medico = Guid.NewGuid();

            var result = await Controller(repo, medico).SalvarConfig(ConfigConsultas.Padrao());

            Assert.IsType<OkObjectResult>(result);
            repo.Verify(r => r.SalvarConsultas(It.IsAny<ConfigConsultas>(), medico), Times.Once);
        }
    }
}
