using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserAccess.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfilePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoStorageKey",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfilePhotoUpdatedAtUtc",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePhotoStorageKey",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoUpdatedAtUtc",
                table: "users");
        }
    }
}
