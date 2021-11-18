using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Investager.Infrastructure.Migrations;

public partial class Currencies : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PortfolioAsset");

        migrationBuilder.DropTable(
            name: "UserStarredAsset");

        migrationBuilder.DropTable(
            name: "Portfolio");

        migrationBuilder.AlterColumn<string>(
            name: "Provider",
            table: "Asset",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.CreateTable(
            name: "Currency",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Code = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                ProviderId = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Currency", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Watchlist",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false),
                DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                UserId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Watchlist", x => x.Id);
                table.ForeignKey(
                    name: "FK_Watchlist_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CurrencyPair",
            columns: table => new
            {
                FirstCurrencyId = table.Column<int>(type: "integer", nullable: false),
                SecondCurrencyId = table.Column<int>(type: "integer", nullable: false),
                Provider = table.Column<string>(type: "text", nullable: true),
                HasTimeData = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CurrencyPair", x => new { x.FirstCurrencyId, x.SecondCurrencyId });
                table.ForeignKey(
                    name: "FK_CurrencyPair_Currency_FirstCurrencyId",
                    column: x => x.FirstCurrencyId,
                    principalTable: "Currency",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CurrencyPair_Currency_SecondCurrencyId",
                    column: x => x.SecondCurrencyId,
                    principalTable: "Currency",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "WatchlistAsset",
            columns: table => new
            {
                WatchlistId = table.Column<int>(type: "integer", nullable: false),
                AssetId = table.Column<int>(type: "integer", nullable: false),
                DisplayOrder = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WatchlistAsset", x => new { x.WatchlistId, x.AssetId });
                table.ForeignKey(
                    name: "FK_WatchlistAsset_Asset_AssetId",
                    column: x => x.AssetId,
                    principalTable: "Asset",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_WatchlistAsset_Watchlist_WatchlistId",
                    column: x => x.WatchlistId,
                    principalTable: "Watchlist",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "WatchlistCurrencyPair",
            columns: table => new
            {
                WatchlistId = table.Column<int>(type: "integer", nullable: false),
                CurrencyPairId = table.Column<int>(type: "integer", nullable: false),
                CurrencyPairFirstCurrencyId = table.Column<int>(type: "integer", nullable: true),
                CurrencyPairSecondCurrencyId = table.Column<int>(type: "integer", nullable: true),
                DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                IsReversed = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WatchlistCurrencyPair", x => new { x.WatchlistId, x.CurrencyPairId });
                table.ForeignKey(
                    name: "FK_WatchlistCurrencyPair_CurrencyPair_CurrencyPairFirstCurrenc~",
                    columns: x => new { x.CurrencyPairFirstCurrencyId, x.CurrencyPairSecondCurrencyId },
                    principalTable: "CurrencyPair",
                    principalColumns: new[] { "FirstCurrencyId", "SecondCurrencyId" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WatchlistCurrencyPair_Watchlist_WatchlistId",
                    column: x => x.WatchlistId,
                    principalTable: "Watchlist",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Currency_Code",
            table: "Currency",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CurrencyPair_SecondCurrencyId",
            table: "CurrencyPair",
            column: "SecondCurrencyId");

        migrationBuilder.CreateIndex(
            name: "IX_Watchlist_UserId",
            table: "Watchlist",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_WatchlistAsset_AssetId",
            table: "WatchlistAsset",
            column: "AssetId");

        migrationBuilder.CreateIndex(
            name: "IX_WatchlistCurrencyPair_CurrencyPairFirstCurrencyId_CurrencyP~",
            table: "WatchlistCurrencyPair",
            columns: new[] { "CurrencyPairFirstCurrencyId", "CurrencyPairSecondCurrencyId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WatchlistAsset");

        migrationBuilder.DropTable(
            name: "WatchlistCurrencyPair");

        migrationBuilder.DropTable(
            name: "CurrencyPair");

        migrationBuilder.DropTable(
            name: "Watchlist");

        migrationBuilder.DropTable(
            name: "Currency");

        migrationBuilder.AlterColumn<string>(
            name: "Provider",
            table: "Asset",
            type: "text",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.CreateTable(
            name: "Portfolio",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false),
                UserId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Portfolio", x => x.Id);
                table.ForeignKey(
                    name: "FK_Portfolio_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserStarredAsset",
            columns: table => new
            {
                UserId = table.Column<int>(type: "integer", nullable: false),
                AssetId = table.Column<int>(type: "integer", nullable: false),
                DisplayOrder = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserStarredAsset", x => new { x.UserId, x.AssetId });
                table.ForeignKey(
                    name: "FK_UserStarredAsset_Asset_AssetId",
                    column: x => x.AssetId,
                    principalTable: "Asset",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserStarredAsset_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

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
            name: "IX_Portfolio_UserId",
            table: "Portfolio",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_PortfolioAsset_AssetId",
            table: "PortfolioAsset",
            column: "AssetId");

        migrationBuilder.CreateIndex(
            name: "IX_UserStarredAsset_AssetId",
            table: "UserStarredAsset",
            column: "AssetId");
    }
}
