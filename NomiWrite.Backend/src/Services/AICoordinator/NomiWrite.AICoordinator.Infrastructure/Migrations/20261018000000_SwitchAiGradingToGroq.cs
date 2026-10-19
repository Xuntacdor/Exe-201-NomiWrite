using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NomiWrite.AICoordinator.Infrastructure.Persistence;

#nullable disable

namespace NomiWrite.AICoordinator.Infrastructure.Migrations
{
    [DbContext(typeof(GradingDbContext))]
    [Migration("20261018000000_SwitchAiGradingToGroq")]
    public partial class SwitchAiGradingToGroq : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "provider_name",
                table: "ai_grading_configs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Groq",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "OpenRouter");

            // This migration is intentionally self-contained (there is no generated
            // designer file), so UpdateData cannot resolve property mappings from a
            // target model. Use typed PostgreSQL SQL for the existing seeded row.
            migrationBuilder.Sql(
                """
                UPDATE ai_grading_configs
                SET provider_name = 'Groq',
                    model_name = 'openai/gpt-oss-120b',
                    fallback_model_name = 'qwen/qwen3.8-27b'
                WHERE id = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01'::uuid;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE ai_grading_configs
                SET provider_name = 'OpenRouter',
                    model_name = 'nvidia/nemotron-3.5-lightning:free',
                    fallback_model_name = 'openrouter/free'
                WHERE id = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01'::uuid;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "provider_name",
                table: "ai_grading_configs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "OpenRouter",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Groq");
        }
    }
}
