using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.AICoordinator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiGradingConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_grading_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Gemini"),
                    model_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    temperature = table.Column<decimal>(type: "numeric", nullable: true),
                    system_prompt_template = table.Column<string>(type: "text", nullable: true),
                    max_output_tokens = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_grading_configs", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "ai_grading_configs",
                columns: new[] { "id", "created_at", "is_active", "max_output_tokens", "model_name", "provider_name", "system_prompt_template", "temperature", "updated_at" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01"), new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "gemini-2.5-flash", "Gemini", null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_grading_configs");
        }
    }
}
