using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NomiWrite.Writing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandWritingPracticeCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "image_url",
                value: "https://nomiwrite.example/assets/prompts/urban-transport-modes.png");

            migrationBuilder.InsertData(
                table: "writing_prompts",
                columns: new[] { "id", "created_at", "difficulty", "image_url", "instructions", "is_active", "max_words", "min_words", "sample_answer", "time_limit_minutes", "title", "updated_at", "writing_type_id" },
                values: new object[] { new Guid("10000000-0000-0000-0000-000000000013"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", null, "Write one well-structured academic paragraph explaining how peer feedback can improve student writing.", true, 160, 90, "Peer feedback can improve student writing because it helps learners notice unclear ideas and weak organization before final submission. By reading a classmate's comments, students can revise with a clearer sense of audience and purpose.", 15, "Benefits Of Peer Feedback", null, new Guid("99999999-9999-9999-9999-999999999999") });

            migrationBuilder.InsertData(
                table: "writing_prompts",
                columns: new[] { "id", "created_at", "difficulty", "image_url", "instructions", "is_active", "is_vip_only", "max_words", "min_words", "sample_answer", "time_limit_minutes", "title", "updated_at", "writing_type_id" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000014"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Advanced", null, "Write a statement of purpose for a master's program in data science, focusing on academic background, project experience, and career goals.", true, true, 650, 400, "My interest in data science began when I used statistical models to analyze student performance in a university project. Since then, I have developed programming, research, and communication skills that I hope to deepen through graduate study.", 45, "Data Science Master's SOP", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("10000000-0000-0000-0000-000000000015"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Advanced", null, "Write a formal academic essay discussing whether digital tools improve or reduce meaningful interaction in university classrooms.", true, true, 700, 450, "Digital tools can improve classroom interaction when they support collaborative research, quick feedback, and inclusive participation. However, their value depends on purposeful teaching design rather than the presence of technology alone.", 45, "Technology And Classroom Interaction", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000015"));

            migrationBuilder.UpdateData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "image_url",
                value: null);
        }
    }
}
