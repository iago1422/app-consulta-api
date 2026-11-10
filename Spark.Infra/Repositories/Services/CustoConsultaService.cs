using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;
using Spark.Domain.Services;

namespace Spark.Infra.Repositories.Services
{
    public class CustoConsultaService : ICustoConsulta
    {
        private readonly DataContext _context;
        private readonly IConfiguracaoRepository _config;

        public CustoConsultaService(DataContext context, IConfiguracaoRepository config)
        {
            _context = context;
            _config = config;
        }

        public async Task<int?> CreditosDoTipo(string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return null;
            return (await _config.ObterConsultas()).Config.CreditosDe(tipo);
        }

        public async Task<int?> CreditosDaFila(string referencia)
        {
            if (!Guid.TryParse(referencia, out var filaId)) return null;
            var tipo = await _context.FilaAtendimento.AsNoTracking()
                .Where(f => f.Id == filaId)
                .Select(f => f.Tipo)
                .FirstOrDefaultAsync();
            return tipo == null ? null : await CreditosDoTipo(tipo);
        }

        public async Task<double> SaldoDoDonoDaFicha(Guid fichaId)
        {
            var dono = await _context.FichaClinicas.AsNoTracking()
                .Where(f => f.Id == fichaId)
                .Select(f => (Guid?)f.UserId)
                .FirstOrDefaultAsync();
            if (dono == null) return 0;
            return await _context.ContasCredito.AsNoTracking()
                .Where(c => c.UserId == dono.Value)
                .Select(c => c.Saldo)
                .FirstOrDefaultAsync();
        }
    }
}
