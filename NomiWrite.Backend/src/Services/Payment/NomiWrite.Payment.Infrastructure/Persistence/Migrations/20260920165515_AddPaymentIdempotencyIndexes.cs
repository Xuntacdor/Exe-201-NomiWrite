using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentIdempotencyIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_refund_requests_payment_order_id",
                table: "refund_requests");

            migrationBuilder.DropIndex(
                name: "ix_payment_transactions_payment_id",
                table: "payment_transactions");

            migrationBuilder.CreateIndex(
                name: "ix_refund_requests_payment_order_id",
                table: "refund_requests",
                column: "payment_order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_transactions_payment_id",
                table: "payment_transactions",
                column: "payment_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_refund_requests_payment_order_id",
                table: "refund_requests");

            migrationBuilder.DropIndex(
                name: "ix_payment_transactions_payment_id",
                table: "payment_transactions");

            migrationBuilder.CreateIndex(
                name: "ix_refund_requests_payment_order_id",
                table: "refund_requests",
                column: "payment_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transactions_payment_id",
                table: "payment_transactions",
                column: "payment_id");
        }
    }
}
