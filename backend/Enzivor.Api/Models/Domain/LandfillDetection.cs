using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Models.Domain
{
    public class LandfillDetection
    {
        public int Id { get; set; }

        public string ImageName { get; set; } = "";
        public string? LandfillName { get; set; }
        public LandfillCategory Type { get; set; }
        public int? StartYear { get; set; }
        public double Confidence { get; set; }
        public double NorthWestLat { get; set; }
        public double NorthWestLon { get; set; }
        public double SouthEastLat { get; set; }
        public double SouthEastLon { get; set; }
        public string? PolygonCoordinates { get; set; }
        public double SurfaceArea { get; set; }
        public string? RegionTag { get; set; }
        public SerbianRegion? Region { get; set; }
        public int? LandfillSiteId { get; set; }
        public LandfillSite? LandfillSite { get; set; }
    }
}