using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Subscription.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpirySweepTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "expiry_warnings_sent",
                table: "user_subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expiry_warnings_sent",
                table: "user_subscriptions");
        }
    }
}
