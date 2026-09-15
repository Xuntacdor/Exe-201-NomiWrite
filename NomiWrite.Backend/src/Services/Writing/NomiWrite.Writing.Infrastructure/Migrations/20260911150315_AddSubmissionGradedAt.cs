using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Writing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionGradedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GradedAt",
                table: "writing_submissions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradedAt",
                table: "writing_submissions");
        }
    }
}
