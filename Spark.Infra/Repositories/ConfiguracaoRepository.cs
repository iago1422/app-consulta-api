using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spark.Domain.Configuracao;
using Spark.Domain.Entities;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;

namespace Spark.Domain.Infra.Repositories
{
    public class ConfiguracaoRepository : IConfiguracaoRepository
    {
        // camelCase, igual ao JSON da API
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private readonly DataContext _context;

        public ConfiguracaoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<ConfigConsultasSalva> ObterConsultas()
        {
            var linha = await _context.Configuracoes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Chave == ConfigConsultas.Chave);
            if (linha == null)
                return new ConfigConsultasSalva(ConfigConsultas.Padrao(), null, null);

            var config = JsonSerializer.Deserialize<ConfigConsultas>(linha.Valor, Json) ?? ConfigConsultas.Padrao();
            return new ConfigConsultasSalva(config, linha.AtualizadoEm, linha.AtualizadoPor);
        }

        public async Task SalvarConsultas(ConfigConsultas config, Guid? autorId)
        {
            var linha = await _context.Configuracoes.FirstOrDefaultAsync(c => c.Chave == ConfigConsultas.Chave);
            if (linha == null)
            {
                linha = new Entities.Configuracao { Chave = ConfigConsultas.Chave };
                _context.Configuracoes.Add(linha);
            }
            linha.Valor = JsonSerializer.Serialize(config, Json);
            linha.AtualizadoEm = DateTime.UtcNow;
            linha.AtualizadoPor = autorId;
            await _context.SaveChangesAsync();
        }
    }
}
