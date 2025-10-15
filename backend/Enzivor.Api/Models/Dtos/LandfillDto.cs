using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Models.Dtos
{
    // dto koji cemo kasnije koristiti u kontroleru za upis u bazu
    public class LandfillDto
    {
        public string ImageName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public double SurfaceArea { get; set; }
        public double NorthWestLat { get; set; }
        public double NorthWestLon { get; set; }
        public double SouthEastLat { get; set; }
        public double SouthEastLon { get; set; }
        public double CenterLat { get; set; }
        public double CenterLon { get; set; }
        public string? PolygonCoordinates { get; set; }

        // Region information from CSV
        public string? RegionFromCsv { get; set; }
        public SerbianRegion? ParsedRegion { get; set; }
    }
}

