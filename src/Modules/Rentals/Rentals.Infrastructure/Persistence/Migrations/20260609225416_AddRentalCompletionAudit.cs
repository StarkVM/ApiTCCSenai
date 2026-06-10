using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentals.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalCompletionAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "completed_by_user_id",
                table: "rentals",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_rentals_completed_by_user_id",
                table: "rentals",
                column: "completed_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_rentals_completed_by_user_id",
                table: "rentals");

            migrationBuilder.DropColumn(
                name: "completed_by_user_id",
                table: "rentals");
        }
    }
}
