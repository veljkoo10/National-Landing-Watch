using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Enzivor.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LandfillSites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PointLat = table.Column<double>(type: "double precision", nullable: true),
                    PointLon = table.Column<double>(type: "double precision", nullable: true),
                    BoundaryGeoJson = table.Column<string>(type: "text", nullable: true),
                    EstimatedAreaM2 = table.Column<double>(type: "double precision", nullable: true),
                    EstimatedVolumeM3 = table.Column<double>(type: "double precision", nullable: true),
                    EstimatedCH4TonsPerYear = table.Column<double>(type: "double precision", nullable: true),
                    EstimatedCO2eTonsPerYear = table.Column<double>(type: "double precision", nullable: true),
                    StartYear = table.Column<int>(type: "integer", nullable: true),
                    AnnualMSWTons = table.Column<double>(type: "double precision", nullable: true),
                    Municipality = table.Column<string>(type: "text", nullable: true),
                    RegionTag = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandfillSites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LandfillDetections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    SurfaceArea = table.Column<double>(type: "double precision", nullable: false),
                    NorthWestLat = table.Column<double>(type: "double precision", nullable: false),
                    NorthWestLon = table.Column<double>(type: "double precision", nullable: false),
                    SouthEastLat = table.Column<double>(type: "double precision", nullable: false),
                    SouthEastLon = table.Column<double>(type: "double precision", nullable: false),
                    PolygonCoordinates = table.Column<string>(type: "text", nullable: true),
                    LandfillSiteId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandfillDetections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandfillDetections_LandfillSites_LandfillSiteId",
                        column: x => x.LandfillSiteId,
                        principalTable: "LandfillSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LandfillDetections_ImageName",
                table: "LandfillDetections",
                column: "ImageName");

            migrationBuilder.CreateIndex(
                name: "IX_LandfillDetections_LandfillSiteId",
                table: "LandfillDetections",
                column: "LandfillSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_LandfillSites_Category",
                table: "LandfillSites",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_LandfillSites_Municipality",
                table: "LandfillSites",
                column: "Municipality");

            migrationBuilder.CreateIndex(
                name: "IX_LandfillSites_RegionTag",
                table: "LandfillSites",
                column: "RegionTag");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandfillDetections");

            migrationBuilder.DropTable(
                name: "LandfillSites");
        }
    }
}
