using Microsoft.EntityFrameworkCore.Migrations;

namespace Spark.Infra.Migrations
{
    public partial class chatFileFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "ConsultasChats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "ConsultasChats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "ConsultasChats",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "ConsultasChats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "ConsultasChats",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "ConsultasChats");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "ConsultasChats");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "ConsultasChats");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "ConsultasChats");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "ConsultasChats");
        }
    }
}
