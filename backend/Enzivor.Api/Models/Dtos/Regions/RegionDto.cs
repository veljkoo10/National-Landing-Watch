namespace Enzivor.Api.Models.Dtos.Regions
{
    public class RegionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Population { get; set; }
        public double AreaKm2 { get; set; }
        public int LandfillCount { get; set; }
        public double Ch4Tons { get; set; }
        public double Co2Tons { get; set; }
        public double TotalWaste { get; set; }
        public string PollutionLevel { get; set; } = string.Empty;
    }
}
