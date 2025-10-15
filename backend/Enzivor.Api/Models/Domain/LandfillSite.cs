using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Models.Domain
{
    public class LandfillSite
    {
        public int Id { get; set; }
        public string? Name { get; set; }                    
        public LandfillCategory Category { get; set; }     
        public CurationStatus Status { get; set; }          

        public double? PointLat { get; set; }              
        public double? PointLon { get; set; }
        public string? BoundaryGeoJson { get; set; }       

        public double? EstimatedAreaM2 { get; set; }
        public double? EstimatedVolumeM3 { get; set; }
        public double? EstimatedCH4TonsPerYear { get; set; }
        public double? EstimatedCO2eTonsPerYear { get; set; }

     
        public int? StartYear { get; set; }                  
        public double? AnnualMSWTons { get; set; }          

        
        public string? Municipality { get; set; }
        public string? RegionTag { get; set; }
        public SerbianRegion? Region { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        public ICollection<LandfillDetection> Detections { get; set; } = new List<LandfillDetection>();
    }
}
