using Enzivor.Api.Models.Dtos;

namespace Enzivor.Api.Services.Interfaces
{
    public interface IProductionLandfillProcessor
    {
        Task<List<LandfillDto>> ProcessProductionData(
          string classificationCsvPath,
          string segmentationCsvPath,
          string metadataSpreadsheetPath);
    }
}
