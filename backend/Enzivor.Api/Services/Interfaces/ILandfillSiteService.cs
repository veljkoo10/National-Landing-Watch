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
    }
}