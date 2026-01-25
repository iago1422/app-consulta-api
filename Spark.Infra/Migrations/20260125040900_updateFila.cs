using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spark.Infra.Migrations
{
    public partial class updateFila : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FilaAtendimento",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FilaAtendimento");
        }
    }
}
