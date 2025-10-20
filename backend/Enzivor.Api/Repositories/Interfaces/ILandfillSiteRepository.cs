using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Repositories.Interfaces
{
    public interface ILandfillSiteRepository
    {
        Task<List<LandfillSite>> GetAllAsync(CancellationToken ct = default);
        Task<LandfillSite?> GetByIdAsync(int id, CancellationToken ct = default);
        Task AddAsync(LandfillSite site, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<LandfillSite> sites, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
        Task DeleteAsync(LandfillSite site, CancellationToken ct = default);

        // Regions
        Task<List<LandfillSite>> GetByRegionAsync(string regionTag, CancellationToken ct = default);
        Task<List<(string RegionTag, double TotalWaste)>> GetTotalWasteByRegionAsync(CancellationToken ct = default);
        Task<List<(LandfillCategory Category, int Count)>> GetLandfillTypesAsync(CancellationToken ct = default);
        Task<List<(int Year, double TotalCH4)>> GetCh4EmissionsOverTimeAsync(CancellationToken ct = default);
        Task<List<LandfillSite>> GetTopLargestLandfillsAsync(int count, CancellationToken ct = default);
        Task<MostImpactedRegionFullDto> GetMostImpactedRegionAsync(CancellationToken ct = default);
        Task<List<(int Year, int LandfillCount)>> GetLandfillGrowthOverYearsAsync(CancellationToken ct = default);
        Task<List<(string RegionTag, double EmissionsPerCapita)>> GetEmissionsPerCapitaAsync(Dictionary<string, int> regionPopulations, CancellationToken ct = default);
        
        // Monitorings
        Task<LandfillSite?> GetByIdWithDetectionsAsync(int id, CancellationToken ct = default);
    }
}
