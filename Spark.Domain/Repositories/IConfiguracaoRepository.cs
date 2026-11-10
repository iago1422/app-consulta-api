using System;
using System.Threading.Tasks;
using Spark.Domain.Configuracao;

namespace Spark.Domain.Repositories
{
    public record ConfigConsultasSalva(ConfigConsultas Config, DateTime? AtualizadoEm, Guid? AtualizadoPor);

    public interface IConfiguracaoRepository
    {
        /// <summary>Configuração salva, ou ConfigConsultas.Padrao() se nunca foi salva.</summary>
        Task<ConfigConsultasSalva> ObterConsultas();
        Task SalvarConsultas(ConfigConsultas config, Guid? autorId);
    }
}
