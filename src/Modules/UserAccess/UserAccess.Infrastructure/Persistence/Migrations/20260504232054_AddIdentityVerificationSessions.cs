using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserAccess.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityVerificationSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "identity_verification_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderSessionId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ProviderSessionUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_verification_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_identity_verification_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_identity_verification_sessions_Provider_ProviderSessionId",
                table: "identity_verification_sessions",
                columns: new[] { "Provider", "ProviderSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identity_verification_sessions_ProviderSessionId",
                table: "identity_verification_sessions",
                column: "ProviderSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identity_verification_sessions_UserId",
                table: "identity_verification_sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_identity_verification_sessions_UserId_Status",
                table: "identity_verification_sessions",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identity_verification_sessions");
        }
    }
}
