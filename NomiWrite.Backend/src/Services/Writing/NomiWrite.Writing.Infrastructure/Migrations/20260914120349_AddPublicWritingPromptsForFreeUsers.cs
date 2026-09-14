using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NomiWrite.Writing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicWritingPromptsForFreeUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "writing_prompts",
                columns: new[] { "id", "created_at", "difficulty", "image_url", "instructions", "is_active", "max_words", "min_words", "sample_answer", "time_limit_minutes", "title", "updated_at", "writing_type_id" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000016"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", null, "Summarize the relationship between a reading passage about a new online course policy and a lecture that questions its benefits.", true, 225, 150, "The reading supports the new online course policy because it offers flexibility and helps students manage their schedules. The lecture challenges this view by arguing that online classes may reduce discussion quality and make it harder for some students to stay motivated.", 20, "Online Course Announcement", null, new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("10000000-0000-0000-0000-000000000017"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", null, "Write concise meeting minutes from notes about a weekly planning meeting, including decisions, owners, action items, and deadlines.", true, 320, 180, "The weekly planning meeting reviewed current project progress, confirmed priority tasks, and assigned owners for design, testing, and client communication. The minutes should clearly record each decision and deadline so the team can follow up efficiently.", 25, "Weekly Planning Meeting Minutes", null, new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("10000000-0000-0000-0000-000000000018"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", null, "Write a personal statement for a computer science scholarship, focusing on motivation, relevant achievements, and future contribution.", true, 600, 350, "My interest in computer science grew from building small applications that solved everyday problems for classmates. A scholarship would help me continue developing technical skills and contribute to projects that make learning more accessible.", 45, "Computer Science Scholarship Statement", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("10000000-0000-0000-0000-000000000019"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", null, "Write a formal academic essay discussing whether group work should be used more often in university courses.", true, 600, 350, "Group work can strengthen university learning because it encourages discussion, shared problem solving, and communication skills. However, instructors need clear assessment criteria to prevent unequal participation.", 45, "Group Work In University Courses", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee") },
                    { new Guid("10000000-0000-0000-0000-000000000020"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", null, "Write a research abstract for a small study investigating the relationship between study habits and exam performance among university students.", true, 250, 150, "This study examines the relationship between study habits and exam performance among university students. Survey responses and course results are analyzed to identify patterns in planning, review frequency, and academic outcomes.", 30, "Abstract For A Study On Study Habits", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000020"));
        }
    }
}
