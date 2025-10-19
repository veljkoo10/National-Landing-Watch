namespace Enzivor.Api.Models.Dtos
{
    public class MostImplactedRegionDto
    {
            private sealed record MostImpactedRegionFullDto(
            string region,
            double totalCh4,
            double totalCo2eq,
            double ch4PerKm2,
            double co2eqPerKm2,
            int population,
            double areaKm2
        );
    }
}
