START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261110033021_PainelAdmin') THEN
    ALTER TABLE "FilaAtendimento" ADD "MedicoId" uuid NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261110033021_PainelAdmin') THEN
    CREATE TABLE "Configuracoes" (
        "Chave" text NOT NULL,
        "Valor" text NULL,
        "AtualizadoEm" timestamp without time zone NULL,
        "AtualizadoPor" uuid NULL,
        CONSTRAINT "PK_Configuracoes" PRIMARY KEY ("Chave")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261110033021_PainelAdmin') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261110033021_PainelAdmin', '5.0.17');
    END IF;
END $EF$;
COMMIT;

