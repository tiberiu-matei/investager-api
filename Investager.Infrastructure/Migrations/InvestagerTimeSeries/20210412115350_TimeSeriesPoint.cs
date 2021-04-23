using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Investager.Infrastructure.Migrations.InvestagerTimeSeries
{
    public partial class TimeSeriesPoint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS timescaledb;");

            migrationBuilder.CreateTable(
                name: "TimeSeriesPoint",
                columns: table => new
                {
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.Sql("SELECT create_hypertable('\"TimeSeriesPoint\"', 'Time', 'Key', 2, create_default_indexes=>FALSE);");
            migrationBuilder.Sql("CREATE INDEX ON \"TimeSeriesPoint\" (\"Key\", \"Time\" DESC);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeSeriesPoint");
        }
    }
}
