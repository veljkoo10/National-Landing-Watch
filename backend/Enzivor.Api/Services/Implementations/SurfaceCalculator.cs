using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    public class SurfaceCalculator : ISurfaceCalculator
    {
        // aproksimira povrsinu deponije u m² na osnovu sjeverozapadne i jugoistocne tacke.
        public double CalculateSurfaceArea(LandfillDto dto)
        {
            var midLat = (dto.NorthWestLat + dto.SouthEastLat) / 2.0;
            var latDistanceMeters = (dto.NorthWestLat - dto.SouthEastLat) * 111_320; 
            var lonDistanceMeters = (dto.SouthEastLon - dto.NorthWestLon) * 111_320 * Math.Cos(midLat * Math.PI / 180);

            latDistanceMeters = Math.Abs(latDistanceMeters);
            lonDistanceMeters = Math.Abs(lonDistanceMeters);

            // povrsina pravougaonika u m²
            return latDistanceMeters * lonDistanceMeters;
        }
    }
}
