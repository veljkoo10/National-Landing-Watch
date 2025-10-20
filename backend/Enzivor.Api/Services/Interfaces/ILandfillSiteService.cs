using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Services.Interfaces
{
    /// <summary>
    /// Defines methods for managing landfill site creation and updates.
    /// </summary>
    public interface ILandfillSiteService
    {
        /// <summary>
        /// Creates landfill sites from unlinked detection data.
        /// </summary>
        /// <param name="minConfidence">Minimum confidence threshold for detections to promote.</param>
        /// <param name="category">Optional landfill category filter.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The number of newly created landfill sites.</returns>
        Task<int> CreateSitesFromUnlinkedDetectionsAsync(
           double? minConfidence = null,
           LandfillCategory? category = null,
           CancellationToken ct = default);
    }
}
