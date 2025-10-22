using Enzivor.Api.Models.Dtos.Landfills;

namespace Enzivor.Api.Services.Interfaces
{
    /// <summary>
    /// Defines methods for converting raw production data into structured landfill DTOs.
    /// </summary>
    public interface IProductionLandfillProcessor
    {
        /// <summary>
        /// Processes classification, segmentation, and metadata CSV files
        /// to generate a list of unified landfill DTOs.
        /// </summary>
        Task<List<LandfillDto>> ProcessProductionData(
          string classificationCsvPath,
          string segmentationCsvPath,
          string metadataSpreadsheetPath);
    }
}