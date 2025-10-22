using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enzivor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStartYearToLandfillDetectionAndLandfillSite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StartYear",
                table: "LandfillDetections",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "LandfillDetections");
        }
    }
}
