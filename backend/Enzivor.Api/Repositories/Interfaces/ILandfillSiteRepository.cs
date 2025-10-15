using Enzivor.Api.Models.Domain;

namespace Enzivor.Api.Repositories.Interfaces
{
    public interface ILandfillSiteRepository
    {
        Task<List<LandfillSite>> GetAllAsync(CancellationToken ct = default);
        Task<LandfillSite?> GetByIdAsync(int id, CancellationToken ct = default);
        Task AddAsync(LandfillSite site, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<LandfillSite> sites, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<LandfillSite?> GetByIdWithDetectionsAsync(int id, CancellationToken ct = default);
        Task DeleteAsync(LandfillSite site, CancellationToken ct = default);

        // prikaz deponija po regionu
        Task<List<LandfillSite>> GetByRegionAsync(string regionTag, CancellationToken ct = default);
        Task<List<string>> GetAvailableRegionsAsync(CancellationToken ct = default);
    }
}
