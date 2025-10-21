namespace Enzivor.Api.Models.Dtos
{
    public class WasteByRegionDto
    {
        public string Name { get; set; } = string.Empty;
        public double TotalWaste { get; set; }

        public WasteByRegionDto() { }

        public WasteByRegionDto(string name, double totalWaste)
        {
            Name = name;
            TotalWaste = totalWaste;
        }
    }
}
