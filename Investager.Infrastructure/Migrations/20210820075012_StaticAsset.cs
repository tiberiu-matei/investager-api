using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Investager.Infrastructure.Migrations
{
    public partial class StaticAsset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPrice",
                table: "Asset");

            migrationBuilder.DropColumn(
                name: "LastPriceUpdate",
                table: "Asset");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "LastPrice",
                table: "Asset",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPriceUpdate",
                table: "Asset",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
