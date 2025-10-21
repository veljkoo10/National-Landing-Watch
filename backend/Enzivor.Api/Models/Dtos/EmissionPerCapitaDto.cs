namespace Enzivor.Api.Models.Dtos
{
    public class EmissionPerCapitaDto
    {
        public string Region { get; set; } = string.Empty;
        public double Ch4PerCapita { get; set; }

        public EmissionPerCapitaDto() { }

        public EmissionPerCapitaDto(string region, double ch4PerCapita)
        {
            Region = region;
            Ch4PerCapita = ch4PerCapita;
        }
    }
}
