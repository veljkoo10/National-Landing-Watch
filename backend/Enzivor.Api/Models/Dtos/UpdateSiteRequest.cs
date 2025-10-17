using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Models.Dtos
{
    public class UpdateSiteRequest
    {
        public string? Name { get; set; }
        public CurationStatus Status { get; set; }
        public string? Municipality { get; set; }
        public string? RegionTag { get; set; }
        public double? EstimatedAreaM2 { get; set; }
        public double? EstimatedVolumeM3 { get; set; }
        public double? EstimatedCH4TonsPerYear { get; set; }
        public double? EstimatedCO2eTonsPerYear { get; set; }
        public int? StartYear { get; set; }
        public double? AnnualMSWTons { get; set; }
    }
}
