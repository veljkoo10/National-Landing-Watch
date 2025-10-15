using Enzivor.Api.Models.Dtos;

namespace Enzivor.Api.Services.Interfaces
{
    public interface ICoordinateConverter
    {
        void ConvertPixelPolygonToGeoCoordinates(
           LandfillDto dto,
           double northWestLat,
           double northWestLon,
           double southEastLat,
           double southEastLon,
           int imageWidthPx,
           int imageHeightPx);
    }
}
