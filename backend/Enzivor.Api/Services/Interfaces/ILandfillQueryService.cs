using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos.Landfills;

namespace Enzivor.Api.Services.Interfaces
{
    /// <summary>
    /// Provides read-only operations for retrieving landfill site data.
    /// </summary>
    public interface ILandfillQueryService
    {
        /// <summary>
        /// Retrieves all landfill sites.
        /// </summary>
        Task<IEnumerable<LandfillSite>> GetAllLandfillsAsync(CancellationToken ct);

        /// <summary>
        /// Retrieves a single landfill by its ID.
        /// </summary>
        Task<LandfillSite?> GetLandfillByIdAsync(int id, CancellationToken ct);

        /// <summary>
        /// Retrieves all landfills within a specific region.
        /// </summary>
        Task<IEnumerable<LandfillSite>> GetLandfillsByRegionAsync(string regionKey, CancellationToken ct);
    }
}
