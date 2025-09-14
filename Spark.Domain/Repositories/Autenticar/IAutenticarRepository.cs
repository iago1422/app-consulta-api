using Spark.Domain.Commands;
using Spark.Domain.Commands.Autenticar;
using Spark.Domain.Entities.Usuarios;
using System.Threading.Tasks;

namespace Spark.Domain.Repositories.Autenticar
{
    public interface IAutenticarRepository
    {
       public Task<EfetuarLogin.Response> GetByUserLogin(EfetuarLogin.Request usuario);
       public Task<AutenticarEmail.Response> AutenticarEmail(AutenticarEmail.Request email);
       public Task<EsqueciMinhaSenha.Response> EsqueciMinhaSenha(EsqueciSenha.Request email);
       public Task<ResetarSenha.Response> ResetarSenha(ResetarSenha.Request resetarsenha);       
       public HealthCheck.Response HealthCheck();       
    }
}