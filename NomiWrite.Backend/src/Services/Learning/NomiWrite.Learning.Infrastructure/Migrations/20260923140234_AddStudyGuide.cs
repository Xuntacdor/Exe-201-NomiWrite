using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Learning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyGuide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "study_guides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetExam = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetBand = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EstimatedBand = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    Strengths = table.Column<string>(type: "jsonb", nullable: false),
                    Weaknesses = table.Column<string>(type: "jsonb", nullable: false),
                    NextSteps = table.Column<string>(type: "jsonb", nullable: false),
                    RecommendedTopic = table.Column<string>(type: "jsonb", nullable: false),
                    AnalyzedEssayCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_guides", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_study_guides_UserId",
                table: "study_guides",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "study_guides");
        }
    }
}
