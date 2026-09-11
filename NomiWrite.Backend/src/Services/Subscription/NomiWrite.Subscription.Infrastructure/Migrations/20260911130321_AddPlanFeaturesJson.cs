using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NomiWrite.Subscription.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanFeaturesJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "features_json",
                table: "subscription_plans",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.UpdateData(
                table: "subscription_plans",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111101"),
                column: "features_json",
                value: "[\"VIP Sample Answers\",\"Priority Support\"]");

            migrationBuilder.UpdateData(
                table: "subscription_plans",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222201"),
                column: "features_json",
                value: "[\"VIP Sample Answers\",\"Priority Support\"]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "features_json",
                table: "subscription_plans");
        }
    }
}
