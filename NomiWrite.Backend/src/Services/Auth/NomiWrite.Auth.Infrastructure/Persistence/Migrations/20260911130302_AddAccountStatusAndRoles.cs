using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Auth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountStatusAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "account_status",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_status",
                table: "users");
        }
    }
}
