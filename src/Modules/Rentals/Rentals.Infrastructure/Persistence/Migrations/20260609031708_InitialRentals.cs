using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentals.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialRentals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rentals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    listing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    renter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_days = table.Column<int>(type: "integer", nullable: false),
                    include_operator = table.Column<bool>(type: "boolean", nullable: false),
                    include_freight = table.Column<bool>(type: "boolean", nullable: false),
                    listing_daily_price_snapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    operator_daily_price_snapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    freight_fixed_price_snapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    machine_subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    operator_subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    freight_subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    approved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rentals", x => x.id);
                    table.CheckConstraint("ck_rentals_amounts_non_negative", "machine_subtotal > 0 AND total_amount > 0");
                    table.CheckConstraint("ck_rentals_freight_prices_non_negative", "freight_fixed_price_snapshot >= 0 AND freight_subtotal >= 0");
                    table.CheckConstraint("ck_rentals_listing_daily_price_positive", "listing_daily_price_snapshot > 0");
                    table.CheckConstraint("ck_rentals_operator_prices_non_negative", "operator_daily_price_snapshot >= 0 AND operator_subtotal >= 0");
                    table.CheckConstraint("ck_rentals_owner_and_renter_must_be_different", "provider_id <> renter_id");
                    table.CheckConstraint("ck_rentals_total_amount_consistency", "total_amount = machine_subtotal + operator_subtotal + freight_subtotal");
                    table.CheckConstraint("ck_rentals_total_days_positive", "total_days > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_rentals_listing_id",
                table: "rentals",
                column: "listing_id");

            migrationBuilder.CreateIndex(
                name: "ix_rentals_listing_id_status",
                table: "rentals",
                columns: new[] { "listing_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_rentals_provider_id",
                table: "rentals",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_rentals_provider_id_status",
                table: "rentals",
                columns: new[] { "provider_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_rentals_renter_id",
                table: "rentals",
                column: "renter_id");

            migrationBuilder.CreateIndex(
                name: "ix_rentals_renter_id_status",
                table: "rentals",
                columns: new[] { "renter_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_rentals_start_date_end_date",
                table: "rentals",
                columns: new[] { "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "ix_rentals_status",
                table: "rentals",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rentals");
        }
    }
}
