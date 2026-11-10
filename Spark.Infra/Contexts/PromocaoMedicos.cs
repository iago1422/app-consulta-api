using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Spark.Domain.Infra.Contexts
{
    /// <summary>
    /// Ao subir, promove para MEDICO as contas PACIENTE indicadas na config/env PROMOVER_MEDICOS
    /// (e-mails ou ids de conta: "a@x.com, 894139c9-..."). O repositório é público: use ids no
    /// promocao-medicos.json versionado e e-mails só na variável de ambiente. Serve para liberar o painel administrativo sem
    /// mexer no banco à mão. Só promove PACIENTE (nunca rebaixa nem troca outro perfil) e é
    /// idempotente: depois da primeira vez, não altera mais nada.
    /// </summary>
    public static class PromocaoMedicos
    {
        public const string Chave = "PROMOVER_MEDICOS";
        private static readonly Guid PerfilMedico = Guid.Parse("efe703c2-2076-4d5b-aa04-aaee90953e49");
        // Ids de produção (conferidos em 2026-11-10): acb3830a = ADMINISTRADOR, e0e938ef = PACIENTE
        private static readonly Guid PerfilPaciente = Guid.Parse("e0e938ef-5ca3-48bc-b5a5-30a0fd0b64ca");
        private static readonly Guid PerfilAdministrador = Guid.Parse("acb3830a-402b-4865-9f70-7b28d39f66ad");

        // Uma vez só: a versão anterior usava o id de ADMINISTRADOR como se fosse PACIENTE e
        // "promoveu" duas contas de ADMINISTRADOR para MEDICO. Devolve o perfil original.
        private static readonly Guid[] RebaixadosPorEngano =
        {
            Guid.Parse("894139c9-8628-497b-82b0-6afe5f61a56e"),
            Guid.Parse("71bf0d03-fa1e-4388-a202-abe9ff56270b"),
        };
        private const string MarcaRestauracao = "fix-perfis-20261110";

        /// <summary>Devolve quantas contas foram restauradas (0 depois da primeira execução).</summary>
        public static int RestaurarAdministradores(DataContext context)
        {
            if (context.Configuracoes.AsNoTracking().Any(c => c.Chave == MarcaRestauracao)) return 0;

            var restaurados = context.Database.ExecuteSqlInterpolated(
                $@"UPDATE ""AspNetUsers"" SET ""PerfilId"" = {PerfilAdministrador}
                   WHERE ""Id"" = ANY({RebaixadosPorEngano}) AND ""PerfilId"" = {PerfilMedico}");

            context.Configuracoes.Add(new Spark.Domain.Entities.Configuracao
            {
                Chave = MarcaRestauracao,
                Valor = "restaura ADMINISTRADOR rebaixado por engano",
                AtualizadoEm = DateTime.UtcNow
            });
            context.SaveChanges();
            return restaurados;
        }

        private static string[] Itens(string lista) =>
            (lista ?? string.Empty).Split(new[] { ',', ';', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>Lista separada por vírgula, ponto e vírgula ou espaço → e-mails no formato do NormalizedEmail do Identity.</summary>
        public static string[] Emails(string lista) =>
            Itens(lista)
                .Select(e => e.Trim().ToUpperInvariant())
                .Where(e => e.Contains('@'))
                .Distinct()
                .ToArray();

        /// <summary>Ids de conta na mesma lista (permite indicar contas sem publicar e-mail no repositório).</summary>
        public static Guid[] Ids(string lista) =>
            Itens(lista)
                .Select(i => Guid.TryParse(i.Trim(), out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToArray();

        /// <summary>Devolve quantas contas foram promovidas.</summary>
        public static int Aplicar(DataContext context, string lista)
        {
            var emails = Emails(lista);
            var ids = Ids(lista);
            if (emails.Length == 0 && ids.Length == 0) return 0;

            return context.Database.ExecuteSqlInterpolated(
                $@"UPDATE ""AspNetUsers"" SET ""PerfilId"" = {PerfilMedico}
                   WHERE (""NormalizedEmail"" = ANY({emails}) OR ""Id"" = ANY({ids})) AND ""PerfilId"" = {PerfilPaciente}");
        }
    }
}
