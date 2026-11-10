using Microsoft.EntityFrameworkCore;

namespace Spark.Domain.Infra.Contexts
{
    /// <summary>
    /// Garante, ao subir a API, o esquema da migration 20261110033021_PainelAdmin
    /// (FilaAtendimento.MedicoId e tabela Configuracoes). As migrations deste projeto são
    /// aplicadas à mão; esta é aditiva e idempotente, então a própria API aplica com a conexão dela.
    /// Se falhar, a API não sobe e o deploy anterior continua no ar.
    /// </summary>
    public static class EsquemaPainelAdmin
    {
        public const string Sql = @"
ALTER TABLE ""FilaAtendimento"" ADD COLUMN IF NOT EXISTS ""MedicoId"" uuid NULL;

CREATE TABLE IF NOT EXISTS ""Configuracoes"" (
    ""Chave"" text NOT NULL,
    ""Valor"" text NULL,
    ""AtualizadoEm"" timestamp without time zone NULL,
    ""AtualizadoPor"" uuid NULL,
    CONSTRAINT ""PK_Configuracoes"" PRIMARY KEY (""Chave"")
);

DO $$
BEGIN
    IF to_regclass('""__EFMigrationsHistory""') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '20261110033021_PainelAdmin') THEN
        INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") VALUES ('20261110033021_PainelAdmin', '5.0.17');
    END IF;
END $$;";

        public static void Garantir(DataContext context) => context.Database.ExecuteSqlRaw(Sql);
    }
}
