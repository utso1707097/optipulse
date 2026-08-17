using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptiPulse.Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExposureEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExposureEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FlagKey = table.Column<string>(type: "TEXT", nullable: false),
                    ExperimentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    VariantKey = table.Column<string>(type: "TEXT", nullable: true),
                    ContextKey = table.Column<string>(type: "TEXT", nullable: true),
                    SnapshotVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExposureEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExposureEvents_FlagKey",
                table: "ExposureEvents",
                column: "FlagKey");

            migrationBuilder.CreateIndex(
                name: "IX_ExposureEvents_Timestamp",
                table: "ExposureEvents",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExposureEvents");
        }
    }
}
