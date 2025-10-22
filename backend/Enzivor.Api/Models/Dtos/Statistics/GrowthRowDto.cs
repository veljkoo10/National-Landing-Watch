namespace Enzivor.Api.Models.Dtos.Statistics
{
    public class GrowthRowDto
    {
        public int Year { get; set; }
        public int LandfillCount { get; set; }

        public GrowthRowDto() { }

        public GrowthRowDto(int year, int landfillCount)
        {
            Year = year;
            LandfillCount = landfillCount;
        }
    }
}
