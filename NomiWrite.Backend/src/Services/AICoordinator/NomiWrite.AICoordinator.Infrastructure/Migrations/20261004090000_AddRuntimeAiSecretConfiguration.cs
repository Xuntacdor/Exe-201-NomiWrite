using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NomiWrite.AICoordinator.Infrastructure.Persistence;

#nullable disable

namespace NomiWrite.AICoordinator.Infrastructure.Migrations
{
    [DbContext(typeof(GradingDbContext))]
    [Migration("20261004090000_AddRuntimeAiSecretConfiguration")]
    public partial class AddRuntimeAiSecretConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "api_key_ciphertext",
                table: "ai_grading_configs",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fallback_model_name",
                table: "ai_grading_configs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ai_grading_configs",
                keyColumn: "id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01"),
                column: "fallback_model_name",
                value: "gemini-2.5-flash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "api_key_ciphertext", table: "ai_grading_configs");
            migrationBuilder.DropColumn(name: "fallback_model_name", table: "ai_grading_configs");
        }
    }
}
