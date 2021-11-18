using Microsoft.EntityFrameworkCore.Migrations;

namespace Investager.Infrastructure.Migrations;

public partial class AssetLastPrice : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<float>(
            name: "LastPrice",
            table: "Asset",
            type: "real",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastPrice",
            table: "Asset");
    }
}
