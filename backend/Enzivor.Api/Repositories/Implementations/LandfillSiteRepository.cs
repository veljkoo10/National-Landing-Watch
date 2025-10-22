using Enzivor.Api.Data;
using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos.Statistics;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Models.Static;
using Enzivor.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enzivor.Api.Repositories.Implementations
{
    public class LandfillSiteRepository : BaseRepository<LandfillSite>, ILandfillSiteRepository
    {
        public LandfillSiteRepository(AppDbContext db) : base(db) { }

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
                .Select(g => new { Year = g.Key, TotalCH4 = g.Sum(s => s.EstimatedCH4Tons ?? 0) })
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

        public async Task<MostImpactedRegionFullDto> GetMostImpactedRegionAsync(CancellationToken ct = default)
        {
            var allLandfills = await GetAllAsync(ct);

            var grouped = allLandfills
                .Where(l => !string.IsNullOrEmpty(l.RegionTag))
                .GroupBy(l => l.RegionTag);

            // Normalize region keys for matching
            var regionPopulations = RegionDefinitions.PopulationByName
                .ToDictionary(kv => kv.Key.ToLower().Replace(" ", ""), kv => kv.Value);

            var regionAreas = RegionDefinitions.AreaByName
                .ToDictionary(kv => kv.Key.ToLower().Replace(" ", ""), kv => kv.Value);

            const double CH4_GWP100 = 28.0;

            var mostImpacted = grouped
                .Select(g =>
                {
                    var regionKey = g.Key?.ToLower().Replace(" ", "") ?? "";
                    var totalCh4 = g.Sum(l => l.EstimatedCH4Tons ?? 0);
                    var population = regionPopulations.TryGetValue(regionKey, out var pop) ? pop : 0;
                    var areaKm2 = regionAreas.TryGetValue(regionKey, out var area) ? area : 0d;
                    var totalCo2eq = totalCh4 * CH4_GWP100;
                    var ch4PerKm2 = areaKm2 > 0 ? totalCh4 / areaKm2 : 0;
                    var co2eqPerKm2 = areaKm2 > 0 ? totalCo2eq / areaKm2 : 0;

                    return new
                    {
                        region = g.Key,
                        totalCh4,
                        totalCo2eq,
                        ch4PerKm2,
                        co2eqPerKm2,
                        population,
                        areaKm2
                    };
                })
                .OrderByDescending(r => r.totalCh4)
                .FirstOrDefault();

            if (mostImpacted == null)
                return new MostImpactedRegionFullDto("Unknown", 0, 0, 0, 0, 0, 0);

            return new MostImpactedRegionFullDto(
                region: mostImpacted.region,
                totalCh4: Math.Round(mostImpacted.totalCh4, 2),
                totalCo2eq: Math.Round(mostImpacted.totalCo2eq, 2),
                ch4PerKm2: Math.Round(mostImpacted.ch4PerKm2, 2),
                co2eqPerKm2: Math.Round(mostImpacted.co2eqPerKm2, 2),
                population: mostImpacted.population,
                areaKm2: mostImpacted.areaKm2
            );
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
            // Normalize keys to match region tags
            var normalizedPop = regionPopulations
                .ToDictionary(kv => kv.Key.ToLower().Replace(" ", ""), kv => kv.Value);

            // Fetch all and filter in-memory (fixes EF translation issue)
            var all = await _db.LandfillSites
                .AsNoTracking()
                .Where(s => !string.IsNullOrEmpty(s.RegionTag))
                .ToListAsync(ct);

            var filtered = all
                .Where(s => s.RegionTag != null && normalizedPop.ContainsKey(s.RegionTag.ToLower().Replace(" ", "")))
                .GroupBy(s => s.RegionTag!.ToLower().Replace(" ", ""))
                .Select(g =>
                {
                    var key = g.Key;
                    var pop = normalizedPop[key];
                    var totalCh4 = g.Sum(x => x.EstimatedCH4Tons ?? 0);
                    var perCapita = pop > 0 ? totalCh4 / pop : 0;
                    return (RegionTag: g.First().RegionTag ?? key, EmissionsPerCapita: perCapita);
                })
                .ToList();

            return filtered;
        }
    }
}