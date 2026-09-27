using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Learning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VocabGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VocabGroupItems",
                columns: table => new
                {
                    VocabGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabSuggestionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabGroupItems", x => new { x.VocabGroupId, x.VocabSuggestionId });
                    table.ForeignKey(
                        name: "FK_VocabGroupItems_VocabGroups_VocabGroupId",
                        column: x => x.VocabGroupId,
                        principalTable: "VocabGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VocabGroupItems_vocab_suggestions_VocabSuggestionId",
                        column: x => x.VocabSuggestionId,
                        principalTable: "vocab_suggestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VocabGroupItems_VocabSuggestionId",
                table: "VocabGroupItems",
                column: "VocabSuggestionId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabGroups_UserId",
                table: "VocabGroups",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VocabGroupItems");

            migrationBuilder.DropTable(
                name: "VocabGroups");
        }
    }
}
