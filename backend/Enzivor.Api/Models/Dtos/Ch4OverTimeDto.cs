namespace Enzivor.Api.Models.Dtos
{
    public class Ch4OverTimeDto
    {
        public List<int> Years { get; set; } = new();
        public List<double> Ch4ByYear { get; set; } = new();

        public Ch4OverTimeDto() { }

        public Ch4OverTimeDto(List<int> years, List<double> ch4ByYear)
        {
            Years = years;
            Ch4ByYear = ch4ByYear;
        }
    }
}
