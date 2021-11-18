using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Investager.Infrastructure.Migrations;

public partial class DisplayName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FirstName",
            table: "User");

        migrationBuilder.DropColumn(
            name: "LastName",
            table: "User");

        migrationBuilder.AddColumn<string>(
            name: "DisplayName",
            table: "User",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AlterColumn<DateTime>(
            name: "LastPriceUpdate",
            table: "Asset",
            type: "timestamp without time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DisplayName",
            table: "User");

        migrationBuilder.AddColumn<string>(
            name: "FirstName",
            table: "User",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LastName",
            table: "User",
            type: "text",
            nullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "LastPriceUpdate",
            table: "Asset",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone");
    }
}
