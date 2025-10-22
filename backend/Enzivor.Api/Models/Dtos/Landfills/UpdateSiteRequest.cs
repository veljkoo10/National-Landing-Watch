using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Models.Dtos.Landfills
{
    public class UpdateSiteRequest
    {
        public string? Name { get; set; }
        public CurationStatus Status { get; set; }
        public string? Municipality { get; set; }
        public string? RegionTag { get; set; }
        public double? EstimatedAreaM2 { get; set; }
        public double? EstimatedVolumeM3 { get; set; }
        public double? EstimatedCH4Tons { get; set; }
        public double? EstimatedCO2eTons { get; set; }
        public int? StartYear { get; set; }
        public double? AnnualMSWTons { get; set; }
    }
}
