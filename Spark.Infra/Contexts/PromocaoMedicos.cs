using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Spark.Domain.Infra.Contexts
{
    /// <summary>
    /// Ao subir, promove para MEDICO as contas PACIENTE cujos e-mails estão na config/env
    /// PROMOVER_MEDICOS ("a@x.com, b@y.com"). Serve para liberar o painel administrativo sem
    /// mexer no banco à mão. Só promove PACIENTE (nunca rebaixa nem troca outro perfil) e é
    /// idempotente: depois da primeira vez, não altera mais nada.
    /// </summary>
    public static class PromocaoMedicos
    {
        public const string Chave = "PROMOVER_MEDICOS";
        private static readonly Guid PerfilMedico = Guid.Parse("efe703c2-2076-4d5b-aa04-aaee90953e49");
        private static readonly Guid PerfilPaciente = Guid.Parse("acb3830a-402b-4865-9f70-7b28d39f66ad");

        /// <summary>Lista separada por vírgula, ponto e vírgula ou espaço → e-mails no formato do NormalizedEmail do Identity.</summary>
        public static string[] Emails(string lista) =>
            (lista ?? string.Empty)
                .Split(new[] { ',', ';', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim().ToUpperInvariant())
                .Where(e => e.Contains('@'))
                .Distinct()
                .ToArray();

        /// <summary>Devolve quantas contas foram promovidas.</summary>
        public static int Aplicar(DataContext context, string lista)
        {
            var emails = Emails(lista);
            if (emails.Length == 0) return 0;

            return context.Database.ExecuteSqlInterpolated(
                $@"UPDATE ""AspNetUsers"" SET ""PerfilId"" = {PerfilMedico}
                   WHERE ""NormalizedEmail"" = ANY({emails}) AND ""PerfilId"" = {PerfilPaciente}");
        }
    }
}
