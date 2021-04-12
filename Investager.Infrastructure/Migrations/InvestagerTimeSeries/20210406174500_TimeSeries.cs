using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Investager.Infrastructure.Migrations.InvestagerTimeSeries
{
    public partial class TimeSeries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS timescaledb;");

            migrationBuilder.CreateTable(
                name: "AssetPrice",
                columns: table => new
                {
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.Sql("SELECT create_hypertable('\"AssetPrice\"', 'Time', 'Key', 2, create_default_indexes=>FALSE);");
            migrationBuilder.Sql("CREATE INDEX ON \"AssetPrice\" (\"Key\", \"Time\" DESC);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetPrice");
        }
    }
}
