using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Investager.Infrastructure.Migrations
{
    public partial class AssetLastPriceUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPriceUpdate",
                table: "Asset",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPriceUpdate",
                table: "Asset");
        }
    }
}
