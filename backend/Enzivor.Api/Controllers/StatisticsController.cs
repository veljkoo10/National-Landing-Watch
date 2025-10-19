using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    public class StatisticsController : ControllerBase
    {
        private readonly ILandfillSiteRepository _siteRepository;

        public StatisticsController(ILandfillSiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        // ---- DTOs (minimal, local to controller) ----
        private sealed record WasteByRegionDto(string name, double totalWaste);
        private sealed record LandfillTypeDto(string name, int count);
        private sealed record Ch4OverTimeDto(List<int> years, List<double> ch4ByYear);
        private sealed record TopLandfillDto(string name, string region, double? areaM2, int? yearCreated);
        private sealed record MostImpactedRegionFullDto(
          string region,
          double totalCh4,
          double totalCo2eq,
          double ch4PerKm2,
          double co2eqPerKm2,
          int population,
          double areaKm2
      );
        private sealed record GrowthRowDto(int year, int landfillCount);

        private sealed record LandfillStatRowDto(
            string landfillName,
            string regionName,
            int year,
            double volumeM3,
            double wasteTons,
            double ch4Tons,
            double co2eqTons
        );

        private static string MapCategoryName(LandfillCategory cat) => cat switch
        {
            LandfillCategory.Illegal => "wild",
            LandfillCategory.NonSanitary => "unsanitary",
            LandfillCategory.Sanitary => "sanitary",
            _ => "unsanitary"
        };

        [HttpGet("total-waste-by-region")]
        public async Task<IActionResult> GetTotalWasteByRegion(CancellationToken ct = default)
        {
            var rows = await _siteRepository.GetTotalWasteByRegionAsync(ct);
            // rows is (RegionTag, TotalWaste) or similar
            var payload = rows.Select(r => new WasteByRegionDto(
                name: r.RegionTag,               
                totalWaste: Math.Round(r.TotalWaste, 2)
            )).ToList();

            return Ok(payload);
        }

        [HttpGet("landfill-types")]
        public async Task<IActionResult> GetLandfillTypes(CancellationToken ct = default)
        {
            var rows = await _siteRepository.GetLandfillTypesAsync(ct);
            var payload = rows.Select(r => new LandfillTypeDto(
                name: MapCategoryName(r.Category),
                count: r.Count
            )).ToList();

            return Ok(payload);
        }

        [HttpGet("ch4-over-time")]
        public async Task<IActionResult> GetCh4EmissionsOverTime(CancellationToken ct = default)
        {
            var rows = await _siteRepository.GetCh4EmissionsOverTimeAsync(ct);
            var years = rows.Select(r => r.Year).OrderBy(y => y).ToList();
            var lookup = rows.ToDictionary(x => x.Year, x => Math.Round(x.TotalCH4, 2));
            var ch4 = years.Select(y => lookup[y]).ToList();

            return Ok(new Ch4OverTimeDto(years, ch4));
        }

        [HttpGet("top3-largest")]
        public async Task<IActionResult> GetTop3LargestLandfills(CancellationToken ct = default)
        {
            var sites = await _siteRepository.GetTopLargestLandfillsAsync(3, ct);
            var payload = sites.Select(s => new TopLandfillDto(
                name: s.Name ?? $"Landfill {s.Id}",
                region: s.RegionTag ?? "Unknown",
                areaM2: s.EstimatedAreaM2,
                yearCreated: s.StartYear
            )).ToList();

            return Ok(payload);
        }

        [HttpGet("most-impacted")]
        public async Task<IActionResult> GetMostImpactedRegion(CancellationToken ct = default)
        {
            var r = await _siteRepository.GetMostImpactedRegionAsync(ct);
            var regionTag = r.RegionTag ?? "Unknown";
            var totalCh4 = Math.Round(r.TotalCH4, 2);

            // Keep these dictionaries here (or move to DB/Config later)
            var regionPopulations = new Dictionary<string, int>
    {
        { "Vojvodina", 1900000 },
        { "Beograd", 1675000 },
        { "Zapadna Srbija", 1200000 },
        { "Šumadija i Pomoravlje", 1000000 },
        { "Istočna Srbija", 800000 },
        { "Južna Srbija", 900000 }
    };

            // Example areas in km² (adjust with your authoritative values)
            var regionAreas = new Dictionary<string, double>
    {
        { "Vojvodina", 21500 },
        { "Beograd", 3226 },
        { "Zapadna Srbija", 26500 },
        { "Šumadija i Pomoravlje", 12600 },
        { "Istočna Srbija", 19300 },
        { "Južna Srbija", 19000 }
    };

            var population = regionPopulations.TryGetValue(regionTag, out var pop) ? pop : 0;
            var areaKm2 = regionAreas.TryGetValue(regionTag, out var area) ? area : 0d;

            const double CH4_GWP100 = 28.0; // change if your methodology differs
            var totalCo2eq = Math.Round(totalCh4 * CH4_GWP100, 2);
            var ch4PerKm2 = areaKm2 > 0 ? Math.Round(totalCh4 / areaKm2, 2) : 0;
            var co2eqPerKm2 = areaKm2 > 0 ? Math.Round(totalCo2eq / areaKm2, 2) : 0;

            var payload = new MostImpactedRegionFullDto(
                region: regionTag,
                totalCh4: totalCh4,
                totalCo2eq: totalCo2eq,
                ch4PerKm2: ch4PerKm2,
                co2eqPerKm2: co2eqPerKm2,
                population: population,
                areaKm2: areaKm2
            );

            return Ok(payload);
        }

        [HttpGet("landfill-growth")]
        public async Task<IActionResult> GetLandfillGrowthOverYears(CancellationToken ct = default)
        {
            var rows = await _siteRepository.GetLandfillGrowthOverYearsAsync(ct);
            var payload = rows.OrderBy(r => r.Year)
                              .Select(r => new GrowthRowDto(r.Year, r.LandfillCount))
                              .ToList();
            return Ok(payload);
        }

        [HttpGet("emissions-per-capita")]
        public async Task<IActionResult> GetEmissionsPerCapita(CancellationToken ct = default)
        {
            var regionPopulations = new Dictionary<string, int>
            {
                { "Vojvodina", 1900000 },
                { "Beograd", 1675000 },
                { "Zapadna Srbija", 1200000 },
                { "Šumadija i Pomoravlje", 1000000 },
                { "Istočna Srbija", 800000 },
                { "Južna Srbija", 900000 }
            };

            var rows = await _siteRepository.GetEmissionsPerCapitaAsync(regionPopulations, ct);
       
            var payload = rows.Select(r => new
            {
                region = r.RegionTag,
                ch4PerCapita = Math.Round(r.EmissionsPerCapita, 6)
            });

            return Ok(payload);
        }

        [HttpGet("landfill-stats")]
        public async Task<IActionResult> GetLandfillStats(CancellationToken ct = default)
        {
            const double CH4_GWP100 = 28.0;

            var rows = await _siteRepository.GetLandfillStatsAsync(ct);

            var payload = rows.Select(r => new LandfillStatRowDto(
                landfillName: string.IsNullOrWhiteSpace(r.SiteName) ? $"Landfill {r.SiteId}" : r.SiteName!,
                regionName: r.RegionTag ?? "Unknown",
                year: r.Year,
                volumeM3: Math.Round(r.VolumeM3, 2),
                wasteTons: Math.Round(r.WasteTons, 2),
                ch4Tons: Math.Round(r.Ch4Tons, 2),
                co2eqTons: Math.Round(r.Ch4Tons * CH4_GWP100, 2)
            )).ToList();

            return Ok(payload);
        }
    }
}
