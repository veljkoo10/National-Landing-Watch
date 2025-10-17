using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enzivor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddKnownLandfillName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LandfillName",
                table: "LandfillDetections",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LandfillName",
                table: "LandfillDetections");
        }
    }
}
