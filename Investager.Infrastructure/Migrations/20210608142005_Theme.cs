using Microsoft.EntityFrameworkCore.Migrations;

namespace Investager.Infrastructure.Migrations;

public partial class Theme : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Theme",
            table: "User",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Theme",
            table: "User");
    }
}
