using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spark.Infra.Migrations
{
    public partial class multiFichas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuarioPacientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsavelId = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoVinculo = table.Column<string>(type: "text", nullable: true),
                    IsResponsavelLegal = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPacientes", x => x.Id);

                    table.ForeignKey(
                        name: "FK_UsuarioPacientes_AspNetUsers_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_UsuarioPacientes_AspNetUsers_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPacientes_PacienteId",
                table: "UsuarioPacientes",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPacientes_ResponsavelId",
                table: "UsuarioPacientes",
                column: "ResponsavelId");

            // Impede duplicação do vínculo (Responsável -> Paciente)
            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPacientes_ResponsavelId_PacienteId",
                table: "UsuarioPacientes",
                columns: new[] { "ResponsavelId", "PacienteId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuarioPacientes");
        }
    }
}
