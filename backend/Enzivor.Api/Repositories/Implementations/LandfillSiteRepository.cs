using Enzivor.Api.Data;
using Enzivor.Api.Models.Domain;
using Enzivor.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enzivor.Api.Repositories.Implementations
{
    public class LandfillSiteRepository : ILandfillSiteRepository
    {
        private readonly AppDbContext _db;

        public LandfillSiteRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<LandfillSite>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.LandfillSites
                .Include(s => s.Detections) 
                .ToListAsync(ct);
        }

        public async Task<LandfillSite?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.LandfillSites
                .Include(s => s.Detections)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<List<LandfillSite>> GetByRegionAsync(string regionTag, CancellationToken ct = default)
        {
            return await _db.LandfillSites
                .Include(s => s.Detections)
                .Where(s => s.RegionTag != null && s.RegionTag.Equals(regionTag, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.EstimatedAreaM2 ?? 0)
                .ToListAsync(ct);
        }

        public async Task<List<string>> GetAvailableRegionsAsync(CancellationToken ct = default)
        {
            return await _db.LandfillSites
                .Where(s => !string.IsNullOrEmpty(s.RegionTag))
                .Select(s => s.RegionTag!)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync(ct);
        }

        public async Task AddAsync(LandfillSite site, CancellationToken ct = default)
        {
            await _db.LandfillSites.AddAsync(site, ct);
        }

        public async Task AddRangeAsync(IEnumerable<LandfillSite> sites, CancellationToken ct = default)
        {
            await _db.LandfillSites.AddRangeAsync(sites, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }

        public async Task<LandfillSite?> GetByIdWithDetectionsAsync(int id, CancellationToken ct = default)
        {
            return await _db.LandfillSites
                 .Include(s => s.Detections)
                 .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task DeleteAsync(LandfillSite site, CancellationToken ct = default)
        {
            _db.LandfillSites.Remove(site);
        }
    }
}
