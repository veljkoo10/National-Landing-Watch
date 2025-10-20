using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Services.Interfaces
{
    /// <summary>
    /// Defines methods for managing landfill site creation, monitoring and statistics.
    /// </summary>
    public interface ILandfillSiteService
    {
        /// <summary>
        /// Creates landfill sites from unlinked detection data.
        /// </summary>
        Task<int> CreateSitesFromUnlinkedDetectionsAsync(
            double? minConfidence = null,
            LandfillCategory? category = null,
            CancellationToken ct = default);

        /// <summary>
        /// Returns all monitorings for a landfill site, including real methane and CO₂ values.
        /// </summary>
        Task<IEnumerable<MonitoringDto>> GetMonitoringsAsync(
            int landfillId,
            CancellationToken ct = default);

        /// <summary>
        /// Returns the latest monitoring data for a landfill site.
        /// </summary>
        Task<MonitoringDto?> GetLatestMonitoringAsync(
            int landfillId,
            CancellationToken ct = default);
    }
}