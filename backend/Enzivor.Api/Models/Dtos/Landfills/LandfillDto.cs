using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Models.Dtos.Landfills
{
    // dto koji cemo kasnije koristiti u kontroleru za upis u bazu
    public class LandfillDto
    {
        // Basic Info
        public string ImageName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;  // From classification
        public double Confidence { get; set; }            // From classification
        public string? PolygonCoordinates { get; set; }   // From segmentation

        // Calculated Geo Data
        public double NorthWestLat { get; set; }
        public double NorthWestLon { get; set; }
        public double SouthEastLat { get; set; }
        public double SouthEastLon { get; set; }
        public double SurfaceArea { get; set; }
        public double CenterLat { get; set; }
        public double CenterLon { get; set; }

        // From Metadata
        public string? KnownLandfillName { get; set; }
        public string? RegionTag { get; set; }
        public SerbianRegion? ParsedRegion { get; set; }
        public int? ZoomLevel { get; set; }
        public int? StartYear { get; set; }

        // Metadata bounds (needed for conversion)
        public double ImageNorthWestLat { get; set; }
        public double ImageNorthWestLon { get; set; }
        public double ImageSouthEastLat { get; set; }
        public double ImageSouthEastLon { get; set; }

        // NEW: FOD Methane Calculation Results
        public double EstimatedDepth { get; set; }
        public double EstimatedDensity { get; set; }
        public double EstimatedMSW { get; set; }
        public double EstimatedVolume { get; set; }
        public double MCF { get; set; }                  
        public double CH4GeneratedTonnes { get; set; }  
        public double CO2EquivalentTonnes { get; set; }
    }
}

