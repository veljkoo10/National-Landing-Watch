using Enzivor.Api.Models.Domain;

namespace Enzivor.Api.Repositories.Interfaces
{
    /// <summary>
    /// Defines data access operations for landfill detection entities.
    /// </summary>
    public interface ILandfillDetectionRepository
    {
        /// <summary>
        /// Retrieves all landfill detections from the database.
        /// </summary>
        Task<List<LandfillDetection>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves all detections that are not yet linked to any landfill site.
        /// </summary>
        Task<List<LandfillDetection>> GetUnlinkedAsync(CancellationToken ct = default);

        /// <summary>
        /// Adds multiple landfill detection records in a single batch operation.
        /// </summary>
        Task AddRangeAsync(IEnumerable<LandfillDetection> detections, CancellationToken ct = default);

        /// <summary>
        /// Retrieves all detections that belong to a specific landfill site.
        /// </summary>
        Task<IEnumerable<LandfillDetection>> GetByLandfillIdAsync(int landfillId, CancellationToken ct = default);
    }
}