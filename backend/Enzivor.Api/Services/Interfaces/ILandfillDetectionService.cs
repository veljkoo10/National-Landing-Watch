using Enzivor.Api.Models.Dtos;

namespace Enzivor.Api.Services.Interfaces
{
    /// <summary>
    /// Orchestrates detection ingestion and basic detection analytics.
    /// </summary>
    public interface ILandfillDetectionService
    {
        Task<ProductionProcessResultDto> ProcessProductionAsync(ProcessProductionRequest req, CancellationToken ct = default);
        Task<DetectionStatsDto> GetStatsAsync(CancellationToken ct = default);
    }
}