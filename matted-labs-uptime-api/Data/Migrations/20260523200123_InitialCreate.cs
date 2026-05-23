using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace matted_labs_uptime_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoredServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IgnoreSslErrors = table.Column<bool>(type: "bit", nullable: false),
                    IntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UptimeChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonitoredServiceId = table.Column<int>(type: "int", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUp = table.Column<bool>(type: "bit", nullable: false),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UptimeChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UptimeChecks_MonitoredServices_MonitoredServiceId",
                        column: x => x.MonitoredServiceId,
                        principalTable: "MonitoredServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UptimeChecks_CheckedAt",
                table: "UptimeChecks",
                column: "CheckedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UptimeChecks_MonitoredServiceId",
                table: "UptimeChecks",
                column: "MonitoredServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UptimeChecks");

            migrationBuilder.DropTable(
                name: "MonitoredServices");
        }
    }
}
