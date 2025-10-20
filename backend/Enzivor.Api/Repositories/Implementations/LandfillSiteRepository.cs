using Enzivor.Api.Data;
using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enzivor.Api.Repositories.Implementations
{
    public class LandfillSiteRepository : BaseRepository<LandfillSite>, ILandfillSiteRepository
    {
        public LandfillSiteRepository(AppDbContext db) : base(db) { }

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

        public async Task<MostImpactedRegionFullDto> GetMostImpactedRegionAsync(CancellationToken ct = default)
        {
            var allLandfills = await GetAllAsync(ct);

            var grouped = allLandfills
                .Where(l => !string.IsNullOrEmpty(l.RegionTag))
                .GroupBy(l => l.RegionTag);

            var regionPopulations = new Dictionary<string, int>
            {
                { "Vojvodina", 1900000 },
                { "Beograd", 1675000 },
                { "Zapadna Srbija", 1200000 },
                { "Šumadija i Pomoravlje", 1000000 },
                { "Istočna Srbija", 800000 },
                { "Južna Srbija", 900000 }
            };

            var regionAreas = new Dictionary<string, double>
            {
                { "Vojvodina", 21500 },
                { "Beograd", 3226 },
                { "Zapadna Srbija", 26500 },
                { "Šumadija i Pomoravlje", 12600 },
                { "Istočna Srbija", 19300 },
                { "Južna Srbija", 19000 }
            };

            const double CH4_GWP100 = 28.0;

            var mostImpacted = grouped
                .Select(g =>
                {
                    var region = g.Key;
                    var totalCh4 = g.Sum(l => l.EstimatedCH4TonsPerYear);
                    var population = regionPopulations.TryGetValue(region, out var pop) ? pop : 0;
                    var areaKm2 = regionAreas.TryGetValue(region, out var area) ? area : 0d;
                    var totalCo2eq = totalCh4 * CH4_GWP100;
                    var ch4PerKm2 = areaKm2 > 0 ? totalCh4 / areaKm2 : 0;
                    var co2eqPerKm2 = areaKm2 > 0 ? totalCo2eq / areaKm2 : 0;

                    return new
                    {
                        region,
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
                totalCh4: Math.Round((double)mostImpacted.totalCh4, 2),
                totalCo2eq: Math.Round((double)mostImpacted.totalCo2eq, 2),
                ch4PerKm2: Math.Round((double)mostImpacted.ch4PerKm2, 2),
                co2eqPerKm2: Math.Round((double)mostImpacted.co2eqPerKm2, 2),
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

        public async Task<LandfillSite?> GetByIdWithDetectionsAsync(int id, CancellationToken ct = default)
        {
            return await _db.LandfillSites
                .Include(s => s.Detections)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }
    }
}