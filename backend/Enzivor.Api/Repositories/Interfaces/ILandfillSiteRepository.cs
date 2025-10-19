using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Repositories.Interfaces
{
    public sealed class LandfillStatRow
    {
        public int SiteId { get; set; }
        public string? SiteName { get; set; }
        public string? RegionTag { get; set; }
        public int Year { get; set; }
        public double VolumeM3 { get; set; }
        public double WasteTons { get; set; }
        public double Ch4Tons { get; set; }
    }
    public interface ILandfillSiteRepository
    {
        Task<List<LandfillSite>> GetAllAsync(CancellationToken ct = default);
        Task<LandfillSite?> GetByIdAsync(int id, CancellationToken ct = default);
        Task AddAsync(LandfillSite site, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<LandfillSite> sites, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

        Task DeleteAsync(LandfillSite site, CancellationToken ct = default);

        // prikaz deponija po regionu
        Task<List<LandfillSite>> GetByRegionAsync(string regionTag, CancellationToken ct = default);
        Task<List<string>> GetAvailableRegionsAsync(CancellationToken ct = default);

        Task<List<(string RegionTag, double TotalWaste)>> GetTotalWasteByRegionAsync(CancellationToken ct = default);
        Task<List<(LandfillCategory Category, int Count)>> GetLandfillTypesAsync(CancellationToken ct = default);
        Task<List<(int Year, double TotalCH4)>> GetCh4EmissionsOverTimeAsync(CancellationToken ct = default);
        Task<List<LandfillSite>> GetTopLargestLandfillsAsync(int count, CancellationToken ct = default);
        Task<(string RegionTag, double TotalCH4)> GetMostImpactedRegionAsync(CancellationToken ct = default);
        Task<List<(int Year, int LandfillCount)>> GetLandfillGrowthOverYearsAsync(CancellationToken ct = default);
        Task<List<(string RegionTag, double EmissionsPerCapita)>> GetEmissionsPerCapitaAsync(Dictionary<string, int> regionPopulations, CancellationToken ct = default);

        Task<IReadOnlyList<LandfillStatRow>> GetLandfillStatsAsync(CancellationToken ct = default);
    }
}
