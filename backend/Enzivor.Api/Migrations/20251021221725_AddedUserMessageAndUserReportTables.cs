using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enzivor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserMessageAndUserReportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LandfillReports",
                table: "LandfillReports");

            migrationBuilder.RenameTable(
                name: "LandfillReports",
                newName: "UserReports");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserReports",
                table: "UserReports",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserReports",
                table: "UserReports");

            migrationBuilder.RenameTable(
                name: "UserReports",
                newName: "LandfillReports");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LandfillReports",
                table: "LandfillReports",
                column: "Id");
        }
    }
}
