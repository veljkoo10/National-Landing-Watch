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
        private sealed record MostImpactedRegionDto(string region, double totalCh4);
        private sealed record GrowthRowDto(int year, int landfillCount);

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
                name: r.RegionTag,               // keep display name as stored in DB
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
            // rows is (Year, TotalCH4)
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
            // repo returns (RegionTag, TotalCH4)
            var payload = new MostImpactedRegionDto(
                region: r.RegionTag ?? "Unknown",
                totalCh4: Math.Round(r.TotalCH4, 2)
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
            // NOTE: Keys here must exactly match DB RegionTag values.
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
            // rows is (RegionTag, EmissionsPerCapita)
            // return a plain, explicit array of { region, ch4PerCapita }
            var payload = rows.Select(r => new
            {
                region = r.RegionTag,
                ch4PerCapita = Math.Round(r.EmissionsPerCapita, 6)
            });

            return Ok(payload);
        }
    }
}
