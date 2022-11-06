using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS14.Issues.Migrations
{
    public partial class ftsindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_SyncedIssue_fts""
                ON ""SyncedIssueTemplate"" USING gin
                (to_tsvector('english'::regconfig, ""Title""))
                TABLESPACE pg_default;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_SyncedIssue_fts");
        }
    }
}
