using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spark.Infra.Migrations
{
    public partial class userAnamnese : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Anamneses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Anamneses_UserId",
                table: "Anamneses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Anamneses_AspNetUsers_UserId",
                table: "Anamneses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anamneses_AspNetUsers_UserId",
                table: "Anamneses");

            migrationBuilder.DropIndex(
                name: "IX_Anamneses_UserId",
                table: "Anamneses");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Anamneses");
        }
    }
}
