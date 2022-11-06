using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SS14.Issues.Migrations
{
    public partial class repoconfig_name : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "Name", table: "RepoConfigs", type: "text", nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Name", "RepoConfigs");
        }
    }
}
