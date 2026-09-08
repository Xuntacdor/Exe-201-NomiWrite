using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.AICoordinator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabularyRestructuringTutorReviewAndFeedbackFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "restructuring_suggestions_json",
                table: "grading_results",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "vocabulary_suggestions_json",
                table: "grading_results",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "grading_feedback_flags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    grading_result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grading_feedback_flags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tutor_review_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    submission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tutor_review_requests", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_grading_feedback_flags_grading_result_id",
                table: "grading_feedback_flags",
                column: "grading_result_id");

            migrationBuilder.CreateIndex(
                name: "ix_grading_feedback_flags_user_id",
                table: "grading_feedback_flags",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tutor_review_requests_user_id",
                table: "tutor_review_requests",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grading_feedback_flags");

            migrationBuilder.DropTable(
                name: "tutor_review_requests");

            migrationBuilder.DropColumn(
                name: "restructuring_suggestions_json",
                table: "grading_results");

            migrationBuilder.DropColumn(
                name: "vocabulary_suggestions_json",
                table: "grading_results");
        }
    }
}
