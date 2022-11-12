using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS14.Issues.Migrations
{
    public partial class syncinsertfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP FUNCTION InsertIntoIssueSyncTable;
                create or replace function InsertIntoIssueSyncTable(repo_key text, id int, number int, title text, url text, status text, excerpt text, repoConfigId uuid) returns int as $$
                declare
                    table_name text;
                begin

                    table_name = 'tmp_' || TableNameFromRepoKey(repo_key);
                    
                    execute format('INSERT INTO %I (""Id"", ""Number"", ""Title"", ""Url"", ""Status"", ""Excerpt"", ""RepoConfigId"")' ||
                                   'values (%L, %L, %L, %L, %L, %L, %L)',
                                   table_name, id, number, title, url, status, excerpt, repoConfigId);
                    return 1;
                end;
                $$ language plpgsql;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION InsertIntoIssueSyncTable;");
        }
    }
}
