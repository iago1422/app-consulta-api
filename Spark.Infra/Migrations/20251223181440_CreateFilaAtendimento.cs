using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spark.Infra.Migrations
{
    public partial class CreateFilaAtendimento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilaAtendimento",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "gen_random_uuid()"),
                    TenantId = table.Column<Guid>(nullable: false),
                    PacienteId = table.Column<Guid>(nullable: false),
                    Status = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    CalledAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilaAtendimento", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilaAtendimento_Next",
                table: "FilaAtendimento",
                columns: new[] { "TenantId", "Status", "CreatedAt" }
            );

            migrationBuilder.Sql(@"
        create unique index uq_fila_paciente_ativo
        on ""FilaAtendimento"" (""TenantId"", ""PacienteId"")
        where ""Status"" in ('WAITING','CALLED');
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop index if exists uq_fila_paciente_ativo;");
            migrationBuilder.DropTable(name: "FilaAtendimento");
        }

    }
}
