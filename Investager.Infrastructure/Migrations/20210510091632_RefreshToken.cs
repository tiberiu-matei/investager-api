using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Investager.Infrastructure.Migrations;

public partial class RefreshToken : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Portfolio_User_UserId",
            table: "Portfolio");

        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "Portfolio",
            type: "integer",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "integer");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Portfolio",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.CreateTable(
            name: "RefreshToken",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                EncodedValue = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                LastUsedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                UserId = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RefreshToken", x => x.Id);
                table.ForeignKey(
                    name: "FK_RefreshToken_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RefreshToken_UserId",
            table: "RefreshToken",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Portfolio_User_UserId",
            table: "Portfolio",
            column: "UserId",
            principalTable: "User",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Portfolio_User_UserId",
            table: "Portfolio");

        migrationBuilder.DropTable(
            name: "RefreshToken");

        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "Portfolio",
            type: "integer",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Portfolio",
            type: "text",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Portfolio_User_UserId",
            table: "Portfolio",
            column: "UserId",
            principalTable: "User",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
