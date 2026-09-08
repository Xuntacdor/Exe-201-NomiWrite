using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NomiWrite.Writing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "writing_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_writing_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "writing_prompts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    writing_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    instructions = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    difficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_writing_prompts", x => x.id);
                    table.ForeignKey(
                        name: "FK_writing_prompts_writing_types_writing_type_id",
                        column: x => x.writing_type_id,
                        principalTable: "writing_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "writing_submissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    writing_prompt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    word_count = table.Column<int>(type: "integer", nullable: false),
                    is_timed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_writing_submissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_writing_submissions_writing_prompts_writing_prompt_id",
                        column: x => x.writing_prompt_id,
                        principalTable: "writing_prompts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "writing_types",
                columns: new[] { "id", "category", "created_at", "description", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "ExamFormat", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Interpret and describe visual information (charts, graphs, tables, diagrams).", true, "IELTS Writing Task 1 Academic", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "ExamFormat", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Write a letter in response to a given situation.", true, "IELTS Writing Task 1 General Training", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "ExamFormat", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Write an essay in response to a point of view, argument or problem.", true, "IELTS Writing Task 2", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "ExamFormat", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Read a passage, listen to a lecture, then write a response that synthesizes both.", true, "TOEFL iBT Integrated Writing", null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "ExamFormat", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Write an essay expressing an opinion on a familiar topic.", true, "TOEFL iBT Independent Writing", null },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Professional", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Write a professional cover letter applying for a job.", true, "Cover Letter", null },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Professional", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Write a clear and concise business email for a workplace scenario.", true, "Business Email", null },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "Professional", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Record accurate and organized minutes for a meeting.", true, "Meeting Minutes", null },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "Academic", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Write a well-structured paragraph on an academic topic.", true, "Paragraph Writing", null },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Academic", new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Write a personal statement or statement of purpose for university applications.", true, "Personal Statement / SOP", null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_writing_prompts_writing_type_id",
                table: "writing_prompts",
                column: "writing_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_writing_submissions_user_id",
                table: "writing_submissions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_writing_submissions_writing_prompt_id",
                table: "writing_submissions",
                column: "writing_prompt_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "writing_submissions");

            migrationBuilder.DropTable(
                name: "writing_prompts");

            migrationBuilder.DropTable(
                name: "writing_types");
        }
    }
}
