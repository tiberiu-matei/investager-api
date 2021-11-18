using Microsoft.EntityFrameworkCore.Migrations;

namespace Investager.Infrastructure.Migrations;

public partial class UserStarredAssets : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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

        migrationBuilder.CreateIndex(
            name: "IX_UserStarredAsset_AssetId",
            table: "UserStarredAsset",
            column: "AssetId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserStarredAsset");
    }
}
