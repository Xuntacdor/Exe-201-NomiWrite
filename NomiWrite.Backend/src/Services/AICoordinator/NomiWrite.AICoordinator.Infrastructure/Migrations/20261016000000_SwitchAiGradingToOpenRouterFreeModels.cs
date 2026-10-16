using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NomiWrite.AICoordinator.Infrastructure.Persistence;

#nullable disable

namespace NomiWrite.AICoordinator.Infrastructure.Migrations
{
    [DbContext(typeof(GradingDbContext))]
    [Migration("20261016000000_SwitchAiGradingToOpenRouterFreeModels")]
    public partial class SwitchAiGradingToOpenRouterFreeModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                oldDefaultValue: "Gemini");

            // This migration is intentionally self-contained (there is no generated
            // designer file), so UpdateData cannot resolve property mappings from a
            // target model. Use typed PostgreSQL SQL for the existing seeded row.
            migrationBuilder.Sql(
                """
                UPDATE ai_grading_configs
                SET provider_name = 'OpenRouter',
                    model_name = 'nvidia/nemotron-3.5-lightning:free',
                    fallback_model_name = 'openrouter/free'
                WHERE id = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01'::uuid;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE ai_grading_configs
                SET provider_name = 'Gemini',
                    model_name = 'gemini-3.6-flash',
                    fallback_model_name = 'gemini-2.5-flash'
                WHERE id = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01'::uuid;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "provider_name",
                table: "ai_grading_configs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Gemini",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "OpenRouter");
        }
    }
}
