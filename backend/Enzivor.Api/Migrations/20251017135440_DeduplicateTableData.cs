using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enzivor.Api.Migrations
{
    /// <inheritdoc />
    public partial class DeduplicateTableData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "EstimatedDensity",
                table: "LandfillSites",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EstimatedDepth",
                table: "LandfillSites",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EstimatedMSW",
                table: "LandfillSites",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MCF",
                table: "LandfillSites",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDensity",
                table: "LandfillSites");

            migrationBuilder.DropColumn(
                name: "EstimatedDepth",
                table: "LandfillSites");

            migrationBuilder.DropColumn(
                name: "EstimatedMSW",
                table: "LandfillSites");

            migrationBuilder.DropColumn(
                name: "MCF",
                table: "LandfillSites");
        }
    }
}
