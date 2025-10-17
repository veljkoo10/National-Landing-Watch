using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enzivor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRegionToDetections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Region",
                table: "LandfillSites",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Region",
                table: "LandfillDetections",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionTag",
                table: "LandfillDetections",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Region",
                table: "LandfillSites");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "LandfillDetections");

            migrationBuilder.DropColumn(
                name: "RegionTag",
                table: "LandfillDetections");
        }
    }
}
