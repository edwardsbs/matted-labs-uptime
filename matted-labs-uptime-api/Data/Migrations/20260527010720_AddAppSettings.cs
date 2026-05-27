using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace matted_labs_uptime_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SmtpHost = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SmtpPort = table.Column<int>(type: "int", nullable: false),
                    SmtpUser = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SmtpPassword = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SmtpFrom = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AlertRecipient = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SmtpEnableSsl = table.Column<bool>(type: "bit", nullable: false),
                    AlertsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");
        }
    }
}
