using Enzivor.Api.Models.Dtos.Regions;

namespace Enzivor.Api.Services.Interfaces
{
    /// <summary>
    /// Defines operations for retrieving and analyzing regional landfill data.
    /// </summary>
    public interface IRegionService
    {
        /// <summary>
        /// Returns a list of all regions with their aggregated emission and waste data.
        /// </summary>
        Task<List<RegionDto>> GetAllRegionsAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves data for a single region by its ID.
        /// </summary>
        Task<RegionDto?> GetRegionByIdAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Retrieves data for a single region by its name.
        /// </summary>
        Task<RegionDto?> GetRegionByNameAsync(string name, CancellationToken ct = default);
    }
}