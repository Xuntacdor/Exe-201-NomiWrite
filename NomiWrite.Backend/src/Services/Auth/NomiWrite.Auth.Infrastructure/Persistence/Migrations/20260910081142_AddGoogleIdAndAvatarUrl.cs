using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleIdAndAvatarUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "users");

            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "users");
        }
    }
}
