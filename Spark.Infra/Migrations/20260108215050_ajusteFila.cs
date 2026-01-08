using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spark.Infra.Migrations
{
    public partial class ajusteFila : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PacienteId",
                table: "FilaAtendimento",
                newName: "FichaId");

            migrationBuilder.AddColumn<Guid>(
                name: "AnamnseId",
                table: "FilaAtendimento",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnamnseId",
                table: "FilaAtendimento");

            migrationBuilder.RenameColumn(
                name: "FichaId",
                table: "FilaAtendimento",
                newName: "PacienteId");
        }
    }
}
