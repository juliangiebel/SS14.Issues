using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS14.Issues.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            
            migrationBuilder.CreateTable(
                name: "RepoConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GhInstallationId = table.Column<int>(type: "integer", nullable: false),
                    GhRepoId = table.Column<int>(type: "integer", nullable: false),
                    GhRepoSearchKey = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    AuthRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepoConfigs", x => x.Id);
                });
            
            migrationBuilder.CreateTable(
                name: "SyncedIssueTemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Excerpt = table.Column<string>(type: "text", defaultValue: "<!--empty-->"),
                    RepoConfigId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncedIssue", x => x.Id);
                    table.ForeignKey(
                        "FK_RepoConfigs_SyncedIssue",
                        x => x.RepoConfigId,
                        "RepoConfigs",
                        "Id"
                    );
                });
            migrationBuilder.Sql("CREATE INDEX \"SyncedIssueTemplate_trgm_idx\" ON \"SyncedIssueTemplate\" USING GIST (\"Title\" gist_trgm_ops);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"SyncedIssueTemplate_trgm_idx\"");
            //migrationBuilder.Sql("DROP EXTENSION pg_trgm");
            
            migrationBuilder.DropForeignKey("FK_RepoConfigs_SyncedIssue", "SyncedIssueTemplate");
            migrationBuilder.DropTable(name: "SyncedIssueTemplate");
            migrationBuilder.DropTable(name: "RepoConfigs");
        }
    }
}
