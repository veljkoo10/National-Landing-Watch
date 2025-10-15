using Enzivor.Api.Models.Dtos;

namespace Enzivor.Api.Services.Interfaces
{
    public interface ILandfillProcessingService
    {
        List<LandfillDto> ProcessLandfills(
           string csvPath,
           double northWestLat,
           double northWestLon,
           double southEastLat,
           double southEastLon,
           int imageWidthPx,
           int imageHeightPx);
    }
}
