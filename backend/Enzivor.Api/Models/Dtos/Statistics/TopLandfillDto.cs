namespace Enzivor.Api.Models.Dtos.Statistics
{
    public class TopLandfillDto
    {
        public string Name { get; set; } = string.Empty;
        public string Region { get; set; } = "Unknown";
        public double? AreaM2 { get; set; }
        public int? YearCreated { get; set; }

        public TopLandfillDto() { }

        public TopLandfillDto(string name, string region, double? areaM2, int? yearCreated)
        {
            Name = name;
            Region = region;
            AreaM2 = areaM2;
            YearCreated = yearCreated;
        }
    }
}
