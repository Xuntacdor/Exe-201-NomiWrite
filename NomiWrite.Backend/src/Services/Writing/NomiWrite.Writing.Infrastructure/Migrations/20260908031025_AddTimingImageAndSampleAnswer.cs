using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Writing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimingImageAndSampleAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deadline_at",
                table: "writing_submissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "submitted_late",
                table: "writing_submissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "writing_prompts",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sample_answer",
                table: "writing_prompts",
                type: "character varying(20000)",
                maxLength: 20000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "time_limit_minutes",
                table: "writing_prompts",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deadline_at",
                table: "writing_submissions");

            migrationBuilder.DropColumn(
                name: "submitted_late",
                table: "writing_submissions");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "writing_prompts");

            migrationBuilder.DropColumn(
                name: "sample_answer",
                table: "writing_prompts");

            migrationBuilder.DropColumn(
                name: "time_limit_minutes",
                table: "writing_prompts");
        }
    }
}
