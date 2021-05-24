using Microsoft.EntityFrameworkCore.Migrations;

namespace Investager.Infrastructure.Migrations
{
    public partial class PortfolioAsset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Portfolio_PortfolioId",
                table: "Asset");

            migrationBuilder.DropForeignKey(
                name: "FK_Portfolio_User_UserId",
                table: "Portfolio");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_User_UserId",
                table: "RefreshToken");

            migrationBuilder.DropIndex(
                name: "IX_Asset_PortfolioId",
                table: "Asset");

            migrationBuilder.DropColumn(
                name: "PortfolioId",
                table: "Asset");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "RefreshToken",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

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

            migrationBuilder.CreateTable(
                name: "PortfolioAsset",
                columns: table => new
                {
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    AssetId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioAsset", x => new { x.PortfolioId, x.AssetId });
                    table.ForeignKey(
                        name: "FK_PortfolioAsset_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioAsset_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioAsset_AssetId",
                table: "PortfolioAsset",
                column: "AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Portfolio_User_UserId",
                table: "Portfolio",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_User_UserId",
                table: "RefreshToken",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Portfolio_User_UserId",
                table: "Portfolio");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_User_UserId",
                table: "RefreshToken");

            migrationBuilder.DropTable(
                name: "PortfolioAsset");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "RefreshToken",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

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

            migrationBuilder.AddColumn<int>(
                name: "PortfolioId",
                table: "Asset",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asset_PortfolioId",
                table: "Asset",
                column: "PortfolioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Portfolio_PortfolioId",
                table: "Asset",
                column: "PortfolioId",
                principalTable: "Portfolio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Portfolio_User_UserId",
                table: "Portfolio",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_User_UserId",
                table: "RefreshToken",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
