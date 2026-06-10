using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentals.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalCancellationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "cancellation_penalty_amount",
                table: "rentals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "cancelled_by_user_id",
                table: "rentals",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_rentals_cancelled_by_user_id",
                table: "rentals",
                column: "cancelled_by_user_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_rentals_cancellation_penalty_non_negative",
                table: "rentals",
                sql: "cancellation_penalty_amount >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_rentals_cancelled_by_user_id",
                table: "rentals");

            migrationBuilder.DropCheckConstraint(
                name: "ck_rentals_cancellation_penalty_non_negative",
                table: "rentals");

            migrationBuilder.DropColumn(
                name: "cancellation_penalty_amount",
                table: "rentals");

            migrationBuilder.DropColumn(
                name: "cancelled_by_user_id",
                table: "rentals");
        }
    }
}
