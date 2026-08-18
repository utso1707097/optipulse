using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptiPulse.Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditImmutabilityTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // T081 — the DATABASE half of append-only. The DbContext guard only protects code
            // going through EF; anything holding the connection string (psql, a raw SQL call, a
            // restore, another service) is unaffected by it. A trigger is enforced by Postgres
            // itself, so the guarantee no longer depends on which client is talking.
            //
            // INSERT is untouched — the table is append-only, not read-only.
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION optipulse_reject_audit_mutation()
                RETURNS TRIGGER AS $$
                BEGIN
                    RAISE EXCEPTION
                        'AuditEntries is append-only: % is not permitted (FR-019).', TG_OP
                        USING ERRCODE = 'restrict_violation';
                END;
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER audit_entries_append_only
                BEFORE UPDATE OR DELETE ON "AuditEntries"
                FOR EACH ROW EXECUTE FUNCTION optipulse_reject_audit_mutation();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP TRIGGER IF EXISTS audit_entries_append_only ON "AuditEntries";""");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS optipulse_reject_audit_mutation();");
        }
    }
}
