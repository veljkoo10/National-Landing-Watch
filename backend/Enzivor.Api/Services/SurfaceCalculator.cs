using Enzivor.Api.Dtos;

namespace Enzivor.Api.Services
{
    public class SurfaceCalculator
    {
        // aproksimira povrsinu deponije u m² na osnovu sjeverozapadne i jugoistocne tacke.
        public double CalculateSurfaceArea(LandfillDto dto)
        {
            // srednja geografska sirina (za izracunavanje sirine metra po stepenu)
            var midLat = (dto.NorthWestLat + dto.SouthEastLat) / 2.0;
            var latDistanceMeters = (dto.NorthWestLat - dto.SouthEastLat) * 111_320; // 1° lat ~ 111.32 km
            var lonDistanceMeters = (dto.SouthEastLon - dto.NorthWestLon) * 111_320 * Math.Cos(midLat * Math.PI / 180);

            // ako je nesto negativno (teoretski ne bi smjelo biti) → apsolutna vrijednost
            latDistanceMeters = Math.Abs(latDistanceMeters);
            lonDistanceMeters = Math.Abs(lonDistanceMeters);

            // povrsina pravougaonika u m²
            return latDistanceMeters * lonDistanceMeters;
        }
    }
}
