using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Writing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptWordLimitsAndVipFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_vip_only",
                table: "writing_prompts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "max_words",
                table: "writing_prompts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_words",
                table: "writing_prompts",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_vip_only",
                table: "writing_prompts");

            migrationBuilder.DropColumn(
                name: "max_words",
                table: "writing_prompts");

            migrationBuilder.DropColumn(
                name: "min_words",
                table: "writing_prompts");
        }
    }
}
