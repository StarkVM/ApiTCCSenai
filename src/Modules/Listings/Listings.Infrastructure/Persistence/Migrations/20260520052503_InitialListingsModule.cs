using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Listings.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialListingsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "listings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DailyPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "listing_images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ListingId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listing_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_listing_images_listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_listing_images_listings_ListingId1",
                        column: x => x.ListingId1,
                        principalTable: "listings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_listing_images_ListingId",
                table: "listing_images",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_listing_images_ListingId_DisplayOrder",
                table: "listing_images",
                columns: new[] { "ListingId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_listing_images_ListingId1",
                table: "listing_images",
                column: "ListingId1");

            migrationBuilder.CreateIndex(
                name: "IX_listing_images_StorageKey",
                table: "listing_images",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_listings_OwnerId",
                table: "listings",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_listings_Status",
                table: "listings",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "listing_images");

            migrationBuilder.DropTable(
                name: "listings");
        }
    }
}
