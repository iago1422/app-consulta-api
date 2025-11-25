using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Spark.Infra.Migrations
{
    public partial class chamadas2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FichaClinicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tendencias = table.Column<List<string>>(type: "text[]", nullable: true),
                    NomeCompleto = table.Column<string>(type: "text", nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Sexo = table.Column<string>(type: "text", nullable: true),
                    CPF = table.Column<string>(type: "text", nullable: true),
                    CartaoSUS = table.Column<string>(type: "text", nullable: true),
                    Endereco = table.Column<string>(type: "text", nullable: true),
                    Telefones = table.Column<List<string>>(type: "text[]", nullable: true),
                    ContatoEmergenciaNome = table.Column<string>(type: "text", nullable: true),
                    ContatoEmergenciaTelefone = table.Column<string>(type: "text", nullable: true),
                    ContatoEmergenciaRelacao = table.Column<string>(type: "text", nullable: true),
                    Alergias = table.Column<List<string>>(type: "text[]", nullable: true),
                    DoencasCronicas = table.Column<List<string>>(type: "text[]", nullable: true),
                    HistoricoCirurgico = table.Column<string>(type: "text", nullable: true),
                    MedicacoesUsoContinuo = table.Column<List<string>>(type: "text[]", nullable: true),
                    HistoricoFamiliar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaClinicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FichaClinicas_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chamadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FichaClinicaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioOrigemId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioDestinoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Solicitacao = table.Column<string>(type: "text", nullable: true),
                    Resposta = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chamadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chamadas_AspNetUsers_UsuarioDestinoId",
                        column: x => x.UsuarioDestinoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chamadas_AspNetUsers_UsuarioOrigemId",
                        column: x => x.UsuarioOrigemId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chamadas_FichaClinicas_FichaClinicaId",
                        column: x => x.FichaClinicaId,
                        principalTable: "FichaClinicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conexoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChamadaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: true),
                    Dispositivo = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conexoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conexoes_Chamadas_ChamadaId",
                        column: x => x.ChamadaId,
                        principalTable: "Chamadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chamadas_FichaClinicaId",
                table: "Chamadas",
                column: "FichaClinicaId");

            migrationBuilder.CreateIndex(
                name: "IX_Chamadas_UsuarioDestinoId",
                table: "Chamadas",
                column: "UsuarioDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Chamadas_UsuarioOrigemId",
                table: "Chamadas",
                column: "UsuarioOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_Conexoes_ChamadaId",
                table: "Conexoes",
                column: "ChamadaId");

            migrationBuilder.CreateIndex(
                name: "IX_FichaClinicas_UserId",
                table: "FichaClinicas",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conexoes");

            migrationBuilder.DropTable(
                name: "Chamadas");

            migrationBuilder.DropTable(
                name: "FichaClinicas");
        }
    }
}
