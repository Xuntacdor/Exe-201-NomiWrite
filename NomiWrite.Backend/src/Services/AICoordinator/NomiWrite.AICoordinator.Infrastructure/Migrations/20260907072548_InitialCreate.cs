using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NomiWrite.AICoordinator.Domain.Entities;

#nullable disable

namespace NomiWrite.AICoordinator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grading_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    submission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    overall_band = table.Column<decimal>(type: "numeric(3,1)", nullable: false),
                    criterion_scores = table.Column<List<CriterionScore>>(type: "jsonb", nullable: false),
                    overall_feedback = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    grammar_errors_json = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grading_results", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_grading_results_submission_id",
                table: "grading_results",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "ix_grading_results_user_id",
                table: "grading_results",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grading_results");
        }
    }
}
