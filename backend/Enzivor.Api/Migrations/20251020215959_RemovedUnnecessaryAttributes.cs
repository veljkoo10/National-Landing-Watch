using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enzivor.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUnnecessaryAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LandfillSites_Municipality",
                table: "LandfillSites");

            migrationBuilder.DropColumn(
                name: "AnnualMSWTons",
                table: "LandfillSites");

            migrationBuilder.DropColumn(
                name: "Municipality",
                table: "LandfillSites");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LandfillSites");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AnnualMSWTons",
                table: "LandfillSites",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Municipality",
                table: "LandfillSites",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "LandfillSites",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LandfillSites_Municipality",
                table: "LandfillSites",
                column: "Municipality");
        }
    }
}
