using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NomiWrite.Writing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedWritingCatalogPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "writing_prompts",
                columns: new[] { "id", "created_at", "difficulty", "image_url", "instructions", "is_active", "max_words", "min_words", "sample_answer", "time_limit_minutes", "title", "updated_at", "writing_type_id" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", null, "The chart compares how commuters travel in three cities. Summarize the main features and make comparisons where relevant.", true, 220, 150, "The chart compares commuter transport choices across three cities. Overall, public transport is the most common option in the largest city, while private cars dominate in the suburban city. Cycling remains the least used mode in all three locations.", 20, "Urban Transport Modes", null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", null, "You recently enrolled in an online course but cannot continue. Write a letter to the course provider explaining the situation and requesting a refund.", true, 220, 150, "Dear Sir or Madam, I am writing to request a refund for the online course I purchased last week. Unfortunately, my work schedule has changed and I can no longer attend the live sessions.", 20, "Request A Course Refund", null, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", null, "Some people believe remote work improves productivity, while others think employees work better in offices. Discuss both views and give your own opinion.", true, 380, 250, "Remote work can improve productivity by reducing commuting time and allowing employees to focus in a comfortable environment. However, offices still provide faster collaboration and clearer team routines.", 40, "Remote Work And Productivity", null, new Guid("33333333-3333-3333-3333-333333333333") }
                });

            migrationBuilder.InsertData(
                table: "writing_prompts",
                columns: new[] { "id", "created_at", "difficulty", "image_url", "instructions", "is_active", "is_vip_only", "max_words", "min_words", "sample_answer", "time_limit_minutes", "title", "updated_at", "writing_type_id" },
                values: new object[] { new Guid("10000000-0000-0000-0000-000000000004"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Advanced", null, "Summarize the relationship between a reading passage about extended library hours and a lecture that challenges the policy.", true, true, 225, 150, "The reading supports extending library hours because students need quiet study space at night. The lecture disagrees, arguing that staffing costs are too high and existing evening usage is limited.", 20, "Campus Library Policy", null, new Guid("44444444-4444-4444-4444-444444444444") });

            migrationBuilder.InsertData(
                table: "writing_prompts",
                columns: new[] { "id", "created_at", "difficulty", "image_url", "instructions", "is_active", "max_words", "min_words", "sample_answer", "time_limit_minutes", "title", "updated_at", "writing_type_id" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000005"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", null, "Do you agree or disagree that people learn more from mistakes than from success? Use specific reasons and examples.", true, 450, 300, "I agree that mistakes often teach people more than success because failure forces reflection. When a project succeeds easily, people may not understand which choices mattered.", 30, "Learning Through Mistakes", null, new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("10000000-0000-0000-0000-000000000006"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", null, "Write a cover letter for a junior marketing associate role, emphasizing communication, campaign support, and willingness to learn.", true, 320, 180, "Dear Hiring Manager, I am excited to apply for the Junior Marketing Associate position. My academic projects and internship experience have helped me build strong communication and campaign coordination skills.", 30, "Junior Marketing Associate Cover Letter", null, new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("10000000-0000-0000-0000-000000000007"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", null, "Write a professional email informing a client that a project milestone will be delayed by three days and proposing a revised timeline.", true, 220, 120, "Dear Client, I am writing to update you on the current project milestone. We need three additional days to complete final quality checks, and I propose delivering the revised milestone on Friday.", 15, "Project Deadline Update", null, new Guid("77777777-7777-7777-7777-777777777777") }
                });

            migrationBuilder.InsertData(
                table: "writing_prompts",
                columns: new[] { "id", "created_at", "difficulty", "image_url", "instructions", "is_active", "is_vip_only", "max_words", "min_words", "sample_answer", "time_limit_minutes", "title", "updated_at", "writing_type_id" },
                values: new object[] { new Guid("10000000-0000-0000-0000-000000000008"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", null, "Write concise meeting minutes from notes about a product launch meeting, including attendees, decisions, action items, and deadlines.", true, true, 320, 180, "Meeting minutes should identify the meeting purpose, attendees, key decisions, and assigned action items. The product launch date was confirmed, while marketing assets and QA checks were assigned to separate owners.", 25, "Product Launch Meeting Minutes", null, new Guid("88888888-8888-8888-8888-888888888888") });

            migrationBuilder.InsertData(
                table: "writing_types",
                columns: new[] { "id", "category", "created_at", "description", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "ExamFormat", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Summarize an academic passage in one sentence for PTE Academic.", true, "PTE Academic Summarize Written Text", null },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "ExamFormat", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Write a structured essay for Cambridge B2 First or similar Cambridge exams.", true, "Cambridge B2 First Essay", null },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "ExamFormat", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Write an opinion essay for the Vietnamese Standardized Test of English Proficiency.", true, "VSTEP Task 2 Essay", null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Academic", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Write a formal academic essay with a clear thesis and supporting arguments.", true, "Academic Essay", null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Academic", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Write a concise abstract summarizing research purpose, method, findings, and implication.", true, "Research Abstract", null }
                });

            migrationBuilder.InsertData(
                table: "writing_prompts",
                columns: new[] { "id", "created_at", "difficulty", "image_url", "instructions", "is_active", "max_words", "min_words", "sample_answer", "time_limit_minutes", "title", "updated_at", "writing_type_id" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000009"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", null, "Summarize a passage about the shift from printed textbooks to digital textbooks in one sentence.", true, 75, 5, "Universities are increasingly adopting digital textbooks because they reduce costs, improve accessibility, and allow faster updates, although some students still prefer printed materials for focused reading.", 10, "Digital Textbooks In Universities", null, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") },
                    { new Guid("10000000-0000-0000-0000-000000000010"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", null, "Write an essay discussing whether schools should organize more educational trips for students.", true, 220, 140, "Educational trips should be used more often because they connect classroom knowledge with real experiences. However, schools must plan them carefully so they remain affordable and relevant.", 40, "School Trips And Learning", null, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc") },
                    { new Guid("10000000-0000-0000-0000-000000000011"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", null, "Some people think cities should spend more money on public parks. To what extent do you agree or disagree?", true, 380, 250, "I largely agree that cities should invest more in public parks because they improve public health, provide social spaces, and make dense urban areas more livable.", 40, "Public Parks In Cities", null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd") }
                });

            migrationBuilder.InsertData(
                table: "writing_prompts",
                columns: new[] { "id", "created_at", "difficulty", "image_url", "instructions", "is_active", "is_vip_only", "max_words", "min_words", "sample_answer", "time_limit_minutes", "title", "updated_at", "writing_type_id" },
                values: new object[] { new Guid("10000000-0000-0000-0000-000000000012"), new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Advanced", null, "Write a research abstract for a small study investigating how mobile learning apps affect vocabulary retention among university students.", true, true, 250, 150, "This study investigates the effect of mobile learning applications on vocabulary retention among university students. Using pre- and post-tests, it compares app-supported practice with conventional review.", 30, "Abstract For A Study On Mobile Learning", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "writing_prompts",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "writing_types",
                keyColumn: "id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"));

            migrationBuilder.DeleteData(
                table: "writing_types",
                keyColumn: "id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                table: "writing_types",
                keyColumn: "id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                table: "writing_types",
                keyColumn: "id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));

            migrationBuilder.DeleteData(
                table: "writing_types",
                keyColumn: "id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"));
        }
    }
}
