using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Listings.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_listing_images_listings_ListingId1",
                table: "listing_images");

            migrationBuilder.DropIndex(
                name: "IX_listing_images_ListingId1",
                table: "listing_images");

            migrationBuilder.DropColumn(
                name: "ListingId1",
                table: "listing_images");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ListingId1",
                table: "listing_images",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_listing_images_ListingId1",
                table: "listing_images",
                column: "ListingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_listing_images_listings_ListingId1",
                table: "listing_images",
                column: "ListingId1",
                principalTable: "listings",
                principalColumn: "Id");
        }
    }
}
