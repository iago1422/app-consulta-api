using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;

namespace Spark.Api.Services
{
    /// <summary>Formato único de GET /config/consultas e PUT /admin/config/consultas.</summary>
    public static class ConfigConsultasResposta
    {
        public static async Task<object> Montar(IConfiguracaoRepository repo, DataContext context)
        {
            var salva = await repo.ObterConsultas();
            string autor = null;
            if (salva.AtualizadoPor.HasValue)
                autor = await context.Usuarios.AsNoTracking()
                    .Where(u => u.Id == salva.AtualizadoPor.Value)
                    .Select(u => u.Nome)
                    .FirstOrDefaultAsync();

            var c = salva.Config;
            return new
            {
                tipos = c.Tipos.Select(t => new { tipo = t.Tipo, rotulo = t.Rotulo, creditos = t.Creditos }),
                pacotes = c.Pacotes.Select(p => new
                {
                    id = p.Id,
                    tipo = p.Tipo,
                    consultas = p.Consultas,
                    preco = p.Preco,
                    ativo = p.Ativo,
                    creditos = c.CreditosDoPacote(p)
                }),
                atualizadoEm = salva.AtualizadoEm.HasValue
                    ? DateTime.SpecifyKind(salva.AtualizadoEm.Value, DateTimeKind.Utc)
                    : (DateTime?)null,
                atualizadoPorNome = autor
            };
        }
    }
}
