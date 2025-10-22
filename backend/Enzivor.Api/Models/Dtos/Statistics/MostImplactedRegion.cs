namespace Enzivor.Api.Models.Dtos.Statistics
{
    public class MostImpactedRegionFullDto
    {
        public string Region { get; set; }
        public double TotalCh4 { get; set; }
        public double TotalCo2eq { get; set; }
        public double Ch4PerKm2 { get; set; }
        public double Co2eqPerKm2 { get; set; }
        public int Population { get; set; }
        public double AreaKm2 { get; set; }

        public MostImpactedRegionFullDto(
            string region,
            double totalCh4,
            double totalCo2eq,
            double ch4PerKm2,
            double co2eqPerKm2,
            int population,
            double areaKm2)
        {
            Region = region;
            TotalCh4 = totalCh4;
            TotalCo2eq = totalCo2eq;
            Ch4PerKm2 = ch4PerKm2;
            Co2eqPerKm2 = co2eqPerKm2;
            Population = population;
            AreaKm2 = areaKm2;
        }
    }
}
