using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundRequestsAndDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "applied_discount_percent",
                table: "payment_orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "refund_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refund_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_refund_requests_payment_orders_payment_order_id",
                        column: x => x.payment_order_id,
                        principalTable: "payment_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_refund_requests_payment_order_id",
                table: "refund_requests",
                column: "payment_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_requests_user_id",
                table: "refund_requests",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refund_requests");

            migrationBuilder.DropColumn(
                name: "applied_discount_percent",
                table: "payment_orders");
        }
    }
}
