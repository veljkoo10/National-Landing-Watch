using Enzivor.Api.Models.Dtos.Detections;

namespace Enzivor.Api.Services.Interfaces
{
    /// <summary>
    /// Defines operations for processing and managing landfill detections.
    /// </summary>
    public interface ILandfillDetectionService
    {
        /// <summary>
        /// Processes detection outputs (classification, segmentation, metadata)
        /// and imports them into the system.
        /// </summary>
        Task<ProductionProcessResultDto> ProcessProductionAsync(ProcessProductionRequest req, CancellationToken ct = default);

        /// <summary>
        /// Retrieves global detection statistics, such as counts and confidence averages.
        /// </summary>
        Task<DetectionStatsDto> GetStatsAsync(CancellationToken ct = default);
    }
}