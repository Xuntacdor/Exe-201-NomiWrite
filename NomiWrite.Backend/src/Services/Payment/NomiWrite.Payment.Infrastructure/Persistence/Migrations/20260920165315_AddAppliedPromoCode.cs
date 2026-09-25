using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppliedPromoCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "applied_promo_code",
                table: "payment_orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "applied_promo_code",
                table: "payment_orders");
        }
    }
}
