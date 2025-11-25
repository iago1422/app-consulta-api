using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spark.Infra.Migrations
{
    public partial class anamnese : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Anamneses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SintomaPrincipal = table.Column<string>(type: "text", nullable: true),
                    TemFebre = table.Column<bool>(type: "boolean", nullable: false),
                    TemTosse = table.Column<bool>(type: "boolean", nullable: false),
                    TemFaltaDeAr = table.Column<bool>(type: "boolean", nullable: false),
                    TemDorDeCabeca = table.Column<bool>(type: "boolean", nullable: false),
                    TemNausea = table.Column<bool>(type: "boolean", nullable: false),
                    TemDiarreia = table.Column<bool>(type: "boolean", nullable: false),
                    TemDorNoPeito = table.Column<bool>(type: "boolean", nullable: false),
                    TemTontura = table.Column<bool>(type: "boolean", nullable: false),
                    TemLesoesNaPele = table.Column<bool>(type: "boolean", nullable: false),
                    SintomasIniciaramEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TomouMedicacao = table.Column<bool>(type: "boolean", nullable: false),
                    Medicacoes = table.Column<string>(type: "text", nullable: true),
                    TemDiabetes = table.Column<bool>(type: "boolean", nullable: false),
                    TemHipertensao = table.Column<bool>(type: "boolean", nullable: false),
                    TemAsma = table.Column<bool>(type: "boolean", nullable: false),
                    TemDoencaCardiaca = table.Column<bool>(type: "boolean", nullable: false),
                    TemDoencaPulmonarCronica = table.Column<bool>(type: "boolean", nullable: false),
                    Imunossuprimido = table.Column<bool>(type: "boolean", nullable: false),
                    TemAlergias = table.Column<bool>(type: "boolean", nullable: false),
                    EstaGravida = table.Column<bool>(type: "boolean", nullable: false),
                    OutroPreexistente = table.Column<bool>(type: "boolean", nullable: false),
                    OutroPreexistenteDescricao = table.Column<string>(type: "text", nullable: true),
                    TeveContatoComDoente = table.Column<bool>(type: "boolean", nullable: false),
                    TeveViagemRecente = table.Column<bool>(type: "boolean", nullable: false),
                    ViagemRecenteDescricao = table.Column<string>(type: "text", nullable: true),
                    TeveTraumaRecente = table.Column<bool>(type: "boolean", nullable: false),
                    TrabalhaEmSaude = table.Column<bool>(type: "boolean", nullable: false),
                    Fumante = table.Column<bool>(type: "boolean", nullable: false),
                    UsoDeAlcool = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anamneses", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anamneses");
        }
    }
}
