using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Models.Dtos
{

    public record LandfillSiteListItemDto(
        int Id, string? Name, LandfillCategory Category,
        double? PointLat, double? PointLon,
        string? Municipality, string? RegionTag);

    public class CreateLandfillSiteDto
    {
        public string? Name { get; set; }
        public LandfillCategory Category { get; set; }
        public double? PointLat { get; set; }
        public double? PointLon { get; set; }
        public string? Municipality { get; set; }
        public string? RegionTag { get; set; }
        public double? EstimatedAreaM2 { get; set; }
    }
}
