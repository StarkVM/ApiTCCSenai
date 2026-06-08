using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Listings.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFleetToListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFleet",
                table: "listings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFleet",
                table: "listings");
        }
    }
}
