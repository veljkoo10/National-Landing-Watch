using Enzivor.Api.Dtos;

namespace Enzivor.Api.Services
{
    public class LandfillProcessingService
    {
        // ovo je servis kojim spajamo ostale servise i pozivace se iz kontrolera

        // Glavna metoda koja: 
        // 1. cita CSV iz Python modela 
        // 2. pretvara poligon iz piksela u geo-koordinate
        // 3. računa povrsinu
        // 4. vraca listu DTO objekata spremnih za bazu

        private readonly CsvPredictionReader _csvReader;
        private readonly CoordinateConverter _coordinateConverter;
        private readonly SurfaceCalculator _surfaceCalculator;

        public LandfillProcessingService()
        {
            _csvReader = new CsvPredictionReader();
            _coordinateConverter = new CoordinateConverter();
            _surfaceCalculator = new SurfaceCalculator();
        }

        public List<LandfillDto> ProcessLandfills(
            string csvPath,
            double northWestLat,
            double northWestLon,
            double southEastLat,
            double southEastLon,
            int imageWidthPx,
            int imageHeightPx)
        {
            // ucitava predikcije iz csv-a
            var predictions = _csvReader.ReadPredictions(csvPath);

            // obradjuje svaku predikciju
            foreach (var dto in predictions)
            {
                // konverzija piksela u geo-koordinate
                _coordinateConverter.ConvertPixelPolygonToGeoCoordinates(
                    dto,
                    northWestLat,
                    northWestLon,
                    southEastLat,
                    southEastLon,
                    imageWidthPx,
                    imageHeightPx
                );

                // racuna povrsinu
                dto.SurfaceArea = _surfaceCalculator.CalculateSurfaceArea(dto);

                // racunanje centralne tacke (pina na mapi)
                // frontend moze jednostavno uraditi: 
                // map.setView([landfill.centerLat, landfill.centerLon], zoomLevel);
                dto.CenterLat = (dto.NorthWestLat + dto.SouthEastLat) / 2.0;
                dto.CenterLon = (dto.NorthWestLon + dto.SouthEastLon) / 2.0;
            }

            // vraca gotovu listu spremnu za bazu
            return predictions;
        }
    }
}

