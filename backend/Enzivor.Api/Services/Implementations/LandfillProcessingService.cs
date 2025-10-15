using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    public class LandfillProcessingService : ILandfillProcessingService
    {

        private readonly ICsvPredictionReader _csvReader;
        private readonly ICoordinateConverter _coordinateConverter;
        private readonly ISurfaceCalculator _surfaceCalculator;

        public LandfillProcessingService(
            ICsvPredictionReader csvReader,
            ICoordinateConverter coordinateConverter,
            ISurfaceCalculator surfaceCalculator)
        {
            _csvReader = csvReader;
            _coordinateConverter = coordinateConverter;
            _surfaceCalculator = surfaceCalculator;
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
            var predictions = _csvReader.ReadPredictions(csvPath);

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

                dto.SurfaceArea = _surfaceCalculator.CalculateSurfaceArea(dto);

                dto.CenterLat = (dto.NorthWestLat + dto.SouthEastLat) / 2.0;
                dto.CenterLon = (dto.NorthWestLon + dto.SouthEastLon) / 2.0;
            }

            // vraca gotovu listu spremnu za bazu
            return predictions;
        }
    }
}

