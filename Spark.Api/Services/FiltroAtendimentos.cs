using System;
using System.Linq;
using Spark.Domain.Entities;

namespace Spark.Api.Services
{
    /// <summary>Filtros do histórico do painel (traduzíveis pelo EF; testados em memória).</summary>
    public static class FiltroAtendimentos
    {
        public static IQueryable<FilaAtendimento> Aplicar(IQueryable<FilaAtendimento> q, DateTime de, DateTime ate, Guid? medicoId, string status)
        {
            q = q.Where(x => x.Status != "WAITING" && x.CreatedAt >= de && x.CreatedAt < ate);

            if (medicoId.HasValue)
                q = q.Where(x => x.MedicoId == medicoId.Value);

            switch (status)
            {
                case "concluida": q = q.Where(x => x.Status == "DONE" && x.CalledAt != null); break;
                case "interrompida": q = q.Where(x => x.Status == "LEFT" && x.CalledAt != null); break;
                case "desistiu": q = q.Where(x => (x.Status == "LEFT" || x.Status == "DONE") && x.CalledAt == null); break;
                case "em_andamento": q = q.Where(x => x.Status == "CALLED"); break;
            }

            return q.OrderByDescending(x => x.CreatedAt);
        }

        public static string Status(FilaAtendimento f)
        {
            if (f.Status == "CALLED") return "em_andamento";
            if (f.CalledAt == null) return "desistiu"; // saiu ou foi fechado pelo worker sem ser chamado
            return f.Status == "DONE" ? "concluida" : "interrompida";
        }
    }
}
