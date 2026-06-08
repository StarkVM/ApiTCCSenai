using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Listings.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddListingDetailsAndOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "listings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "freight_available",
                table: "listings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "freight_fixed_price",
                table: "listings",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "operator_available",
                table: "listings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "operator_daily_price",
                table: "listings",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "pickup_city",
                table: "listings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "pickup_complement",
                table: "listings",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pickup_district",
                table: "listings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "pickup_number",
                table: "listings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "pickup_state",
                table: "listings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "pickup_street",
                table: "listings",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "pickup_zip_code",
                table: "listings",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_listings_Category",
                table: "listings",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_listings_Category",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "freight_available",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "freight_fixed_price",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "operator_available",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "operator_daily_price",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "pickup_city",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "pickup_complement",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "pickup_district",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "pickup_number",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "pickup_state",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "pickup_street",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "pickup_zip_code",
                table: "listings");
        }
    }
}
