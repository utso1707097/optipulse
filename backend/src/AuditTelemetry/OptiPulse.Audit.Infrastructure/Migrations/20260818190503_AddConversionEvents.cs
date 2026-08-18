using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OptiPulse.Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConversionEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversionEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FlagKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ExperimentId = table.Column<Guid>(type: "uuid", nullable: true),
                    VariantKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ContextKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Goal = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversionEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversionEvents_FlagKey",
                table: "ConversionEvents",
                column: "FlagKey");

            migrationBuilder.CreateIndex(
                name: "IX_ConversionEvents_IdempotencyKey",
                table: "ConversionEvents",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversionEvents");
        }
    }
}
