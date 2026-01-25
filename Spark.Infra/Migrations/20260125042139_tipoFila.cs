using Microsoft.EntityFrameworkCore.Migrations;

namespace Spark.Infra.Migrations
{
    public partial class tipoFila : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "FilaAtendimento",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "FilaAtendimento");
        }
    }
}
