using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptiPulse.IdentityAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    KeyHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAccounts_KeyHash",
                table: "ServiceAccounts",
                column: "KeyHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceAccounts");
        }
    }
}
