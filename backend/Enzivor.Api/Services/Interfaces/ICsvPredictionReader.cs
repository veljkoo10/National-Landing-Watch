using Enzivor.Api.Models.Dtos;

namespace Enzivor.Api.Services.Interfaces
{
    public interface ICsvPredictionReader
    {
        List<LandfillDto> ReadPredictions(string csvPath);
    }
}
