using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enzivor.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameCH4AndCO2Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EstimatedCO2eTonsPerYear",
                table: "LandfillSites",
                newName: "EstimatedCO2eTons");

            migrationBuilder.RenameColumn(
                name: "EstimatedCH4TonsPerYear",
                table: "LandfillSites",
                newName: "EstimatedCH4Tons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EstimatedCO2eTons",
                table: "LandfillSites",
                newName: "EstimatedCO2eTonsPerYear");

            migrationBuilder.RenameColumn(
                name: "EstimatedCH4Tons",
                table: "LandfillSites",
                newName: "EstimatedCH4TonsPerYear");
        }
    }
}
