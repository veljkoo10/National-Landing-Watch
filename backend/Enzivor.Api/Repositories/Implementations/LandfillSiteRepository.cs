using Enzivor.Api.Data;
using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enzivor.Api.Repositories.Implementations
{
    public class LandfillSiteRepository : ILandfillSiteRepository
    {
        private readonly AppDbContext _db;

        public LandfillSiteRepository(AppDbContext db) => _db = db;

        public async Task<List<LandfillSite>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.LandfillSites
                .AsNoTracking()
                // .Include(s => s.Detections) // <-- include only where needed
                .ToListAsync(ct);
        }

        public async Task<LandfillSite?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.LandfillSites
                .AsNoTracking()
                .Include(s => s.Detections) // ← IMPORTANT so MonitoringsController has data
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        // Keep the name for now; it expects a canonical, normalized region KEY (e.g., "istocnasrbija")
        public async Task<List<LandfillSite>> GetByRegionAsync(string regionTag, CancellationToken ct = default)
        {
            var tag = regionTag.ToLower().Replace(" ", "");
            return await _db.LandfillSites
                .AsNoTracking()
                .Where(s => s.RegionTag != null &&
                            s.RegionTag.ToLower().Replace(" ", "") == tag)
                .OrderByDescending(s => s.EstimatedAreaM2 ?? 0)
                .ToListAsync(ct);
        }



        public async Task<List<string>> GetAvailableRegionsAsync(CancellationToken ct = default)
        {
            // Return the canonical keys as stored (already normalized)
            return await _db.LandfillSites
                .AsNoTracking()
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

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);

        // Not really async; keep it simple
        public Task DeleteAsync(LandfillSite site, CancellationToken ct = default)
        {
            _db.LandfillSites.Remove(site);
            return Task.CompletedTask;
        }

        public async Task<List<(string RegionTag, double TotalWaste)>> GetTotalWasteByRegionAsync(CancellationToken ct = default)
        {
            var rows = await _db.LandfillSites
                .AsNoTracking()
                .Where(s => !string.IsNullOrEmpty(s.RegionTag))
                .GroupBy(s => s.RegionTag!)
                .Select(g => new { RegionTag = g.Key, TotalWaste = g.Sum(s => s.EstimatedMSW ?? 0) })
                .ToListAsync(ct);

            return rows.Select(r => (r.RegionTag, r.TotalWaste)).ToList();
        }

        public async Task<List<(LandfillCategory Category, int Count)>> GetLandfillTypesAsync(CancellationToken ct = default)
        {
            var rows = await _db.LandfillSites
                .AsNoTracking()
                .GroupBy(s => s.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return rows.Select(r => (r.Category, r.Count)).ToList();
        }

        public async Task<List<(int Year, double TotalCH4)>> GetCh4EmissionsOverTimeAsync(CancellationToken ct = default)
        {
            var rows = await _db.LandfillSites
                .AsNoTracking()
                .Where(s => s.StartYear.HasValue)
                .GroupBy(s => s.StartYear!.Value)
                .Select(g => new { Year = g.Key, TotalCH4 = g.Sum(s => s.EstimatedCH4TonsPerYear ?? 0) })
                .OrderBy(r => r.Year)
                .ToListAsync(ct);

            return rows.Select(r => (r.Year, r.TotalCH4)).ToList();
        }

        public async Task<List<LandfillSite>> GetTopLargestLandfillsAsync(int count, CancellationToken ct = default)
        {
            return await _db.LandfillSites
                .AsNoTracking()
                .OrderByDescending(s => s.EstimatedAreaM2 ?? 0)
                .Take(count)
                .ToListAsync(ct);
        }

        public async Task<(string RegionTag, double TotalCH4)> GetMostImpactedRegionAsync(CancellationToken ct = default)
        {
            var r = await _db.LandfillSites
                .AsNoTracking()
                .Where(s => !string.IsNullOrEmpty(s.RegionTag))
                .GroupBy(s => s.RegionTag!)
                .Select(g => new { RegionTag = g.Key, TotalCH4 = g.Sum(s => s.EstimatedCH4TonsPerYear ?? 0) })
                .OrderByDescending(x => x.TotalCH4)
                .FirstOrDefaultAsync(ct);

            return r is null ? ("unknown", 0) : (r.RegionTag, r.TotalCH4);
        }

        public async Task<List<(int Year, int LandfillCount)>> GetLandfillGrowthOverYearsAsync(CancellationToken ct = default)
        {
            var rows = await _db.LandfillSites
                .AsNoTracking()
                .Where(s => s.StartYear.HasValue)
                .GroupBy(s => s.StartYear!.Value)
                .Select(g => new { Year = g.Key, LandfillCount = g.Count() })
                .OrderBy(r => r.Year)
                .ToListAsync(ct);

            return rows.Select(r => (r.Year, r.LandfillCount)).ToList();
        }

        public async Task<List<(string RegionTag, double EmissionsPerCapita)>> GetEmissionsPerCapitaAsync(
            Dictionary<string, int> regionPopulations,
            CancellationToken ct = default)
        {
            // Expect keys of regionPopulations to be canonical region keys (same as RegionTag)
            var rows = await _db.LandfillSites
                .AsNoTracking()
                .Where(s => !string.IsNullOrEmpty(s.RegionTag) && regionPopulations.ContainsKey(s.RegionTag!))
                .GroupBy(s => s.RegionTag!)
                .Select(g => new { RegionTag = g.Key, TotalCH4 = g.Sum(s => s.EstimatedCH4TonsPerYear ?? 0) })
                .ToListAsync(ct);

            return rows.Select(r =>
            {
                var pop = regionPopulations[r.RegionTag];
                var perCapita = pop > 0 ? r.TotalCH4 / pop : 0;
                return (r.RegionTag, perCapita);
            }).ToList();
        }

        public async Task<IReadOnlyList<LandfillStatRow>> GetLandfillStatsAsync(CancellationToken ct = default)
        {
            const double BULK_DENSITY_T_PER_M3 = 0.6;

            var rows = await _db.LandfillSites
                .AsNoTracking()
                .Select(s => new LandfillStatRow
                {
                    SiteId = s.Id,
                    SiteName = s.Name,
                    RegionTag = s.RegionTag,
                    Year = s.StartYear ?? 0,
                    WasteTons = s.EstimatedMSW ?? 0,
                    Ch4Tons = s.EstimatedCH4TonsPerYear ?? 0,
                    VolumeM3 = (s.EstimatedMSW ?? 0) > 0
                                ? (s.EstimatedMSW!.Value / BULK_DENSITY_T_PER_M3)
                                : 0
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.SiteName)
                .ToListAsync(ct);

            return rows;
        }
    }


}
