namespace Enzivor.Api.Models.Dtos
{
    
        public class ShowLandfillDto
        {
            // Basic info
            public int Id { get; set; }                  // Unique identifier
            public string? Name { get; set; }            // Known landfill name (if available)

            // Location & region
            public string RegionKey { get; set; } = string.Empty; // e.g. "Vojvodina", "Belgrade"
            public double Latitude { get; set; }                  // Center latitude
            public double Longitude { get; set; }                 // Center longitude

            // Classification & size
            public string Type { get; set; } = string.Empty;      // sanitary | unsanitary | wild
            public string Size { get; set; } = string.Empty;      // small | medium | large
            public int YearCreated { get; set; }

            // Optional calculated info
            public double? AreaM2 { get; set; }                   // Surface area if available
        }
 
}
