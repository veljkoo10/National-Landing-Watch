using Enzivor.Api.Models.Dtos.Statistics;

namespace Enzivor.Api.Services.Interfaces
{
    /// <summary>
    /// Defines methods for computing global landfill statistics and analytical insights.
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// Returns total waste quantities aggregated by region.
        /// </summary>
        Task<List<WasteByRegionDto>> GetTotalWasteByRegionAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns landfill distribution grouped by landfill type.
        /// </summary>
        Task<List<LandfillTypeDto>> GetLandfillTypesAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns yearly methane emission trends.
        /// </summary>
        Task<Ch4OverTimeDto> GetCh4OverTimeAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns a list of the top largest landfills by area.
        /// </summary>
        Task<List<TopLandfillDto>> GetTopLargestLandfillsAsync(int count = 3, CancellationToken ct = default);

        /// <summary>
        /// Returns the region with the highest environmental impact.
        /// </summary>
        Task<MostImpactedRegionFullDto> GetMostImpactedRegionAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns landfill growth statistics over multiple years.
        /// </summary>
        Task<List<GrowthRowDto>> GetLandfillGrowthAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns methane emissions normalized by population for each region.
        /// </summary>
        Task<List<EmissionPerCapitaDto>> GetEmissionsPerCapitaAsync(CancellationToken ct = default);
    }
}