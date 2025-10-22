using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos.Statistics;
using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Repositories.Interfaces
{
    /// <summary>
    /// Defines data access operations for landfill site entities,
    /// including regional and statistical queries.
    /// </summary>
    public interface ILandfillSiteRepository
    {
        /// <summary>
        /// Retrieves all landfill sites.
        /// </summary>
        Task<List<LandfillSite>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves a landfill site by its unique ID.
        /// </summary>
        Task<LandfillSite?> GetByIdAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Adds a single landfill site to the database.
        /// </summary>
        Task AddAsync(LandfillSite site, CancellationToken ct = default);

        /// <summary>
        /// Adds multiple landfill sites in a batch operation.
        /// </summary>
        Task AddRangeAsync(IEnumerable<LandfillSite> sites, CancellationToken ct = default);

        /// <summary>
        /// Persists all pending changes to the database.
        /// </summary>
        Task SaveChangesAsync(CancellationToken ct = default);

        /// <summary>
        /// Deletes a landfill site from the database.
        /// </summary>
        Task DeleteAsync(LandfillSite site, CancellationToken ct = default);

        // -------------------- Regional Queries --------------------

        /// <summary>
        /// Retrieves all landfill sites for a given region tag.
        /// </summary>
        Task<List<LandfillSite>> GetByRegionAsync(string regionTag, CancellationToken ct = default);

        /// <summary>
        /// Returns the total waste quantity aggregated by region.
        /// </summary>
        Task<List<(string RegionTag, double TotalWaste)>> GetTotalWasteByRegionAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns the count of landfills grouped by category type.
        /// </summary>
        Task<List<(LandfillCategory Category, int Count)>> GetLandfillTypesAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns total methane emissions grouped by year.
        /// </summary>
        Task<List<(int Year, double TotalCH4)>> GetCh4EmissionsOverTimeAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves the top N largest landfills by surface area.
        /// </summary>
        Task<List<LandfillSite>> GetTopLargestLandfillsAsync(int count, CancellationToken ct = default);

        /// <summary>
        /// Returns detailed statistics about the most impacted region.
        /// </summary>
        Task<MostImpactedRegionFullDto> GetMostImpactedRegionAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns landfill growth statistics by year.
        /// </summary>
        Task<List<(int Year, int LandfillCount)>> GetLandfillGrowthOverYearsAsync(CancellationToken ct = default);

        /// <summary>
        /// Calculates methane emissions per capita for each region.
        /// </summary>
        Task<List<(string RegionTag, double EmissionsPerCapita)>> GetEmissionsPerCapitaAsync(
            Dictionary<string, int> regionPopulations,
            CancellationToken ct = default);
    }
}