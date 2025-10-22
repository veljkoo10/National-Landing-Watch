using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;
using Enzivor.Api.Models.Static;
using Enzivor.Api.Models.Dtos.Statistics;

namespace Enzivor.Api.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ILandfillSiteRepository _siteRepository;

        public StatisticsService(ILandfillSiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        public async Task<List<WasteByRegionDto>> GetTotalWasteByRegionAsync(CancellationToken ct = default)
        {
            var rows = await _siteRepository.GetTotalWasteByRegionAsync(ct);

            return rows
                .Select(r => new WasteByRegionDto(
                    name: r.RegionTag ?? "Unknown",
                    totalWaste: Math.Round(r.TotalWaste, 2)))
                .ToList();
        }

        public async Task<List<LandfillTypeDto>> GetLandfillTypesAsync(CancellationToken ct = default)
        {
            var rows = await _siteRepository.GetLandfillTypesAsync(ct);

            return rows
                .Select(r => new LandfillTypeDto(
                    name: MapCategoryName(r.Category),
                    count: r.Count))
                .ToList();
        }

        public async Task<Ch4OverTimeDto> GetCh4OverTimeAsync(CancellationToken ct = default)
        {
            var rows = await _siteRepository.GetCh4EmissionsOverTimeAsync(ct);
            var years = rows.Select(r => r.Year).OrderBy(y => y).ToList();
            var lookup = rows.ToDictionary(x => x.Year, x => Math.Round(x.TotalCH4, 2));
            var ch4 = years.Select(y => lookup[y]).ToList();

            return new Ch4OverTimeDto(years, ch4);
        }

        public async Task<List<TopLandfillDto>> GetTopLargestLandfillsAsync(int count = 3, CancellationToken ct = default)
        {
            var sites = await _siteRepository.GetTopLargestLandfillsAsync(count, ct);

            return sites
                .Select(s => new TopLandfillDto(
                    name: s.Name ?? $"Landfill {s.Id}",
                    region: s.RegionTag ?? "Unknown",
                    areaM2: s.EstimatedAreaM2,
                    yearCreated: s.StartYear))
                .ToList();
        }

        public async Task<MostImpactedRegionFullDto> GetMostImpactedRegionAsync(CancellationToken ct = default)
        {
            var dto = await _siteRepository.GetMostImpactedRegionAsync(ct);
            return dto;
        }

        public async Task<List<GrowthRowDto>> GetLandfillGrowthAsync(CancellationToken ct = default)
        {
            var rows = await _siteRepository.GetLandfillGrowthOverYearsAsync(ct);

            return rows
                .OrderBy(r => r.Year)
                .Select(r => new GrowthRowDto(r.Year, r.LandfillCount))
                .ToList();
        }

        public async Task<List<EmissionPerCapitaDto>> GetEmissionsPerCapitaAsync(CancellationToken ct = default)
        {
            var regionPopulations = RegionDefinitions.PopulationByName;
            var rows = await _siteRepository.GetEmissionsPerCapitaAsync(regionPopulations, ct);

            return rows
                .Select(r => new EmissionPerCapitaDto(
                    region: r.RegionTag,
                    ch4PerCapita: Math.Round(r.EmissionsPerCapita, 6)))
                .ToList();
        }

        private static string MapCategoryName(LandfillCategory cat) => cat switch
        {
            LandfillCategory.Illegal => "wild",
            LandfillCategory.NonSanitary => "unsanitary",
            LandfillCategory.Sanitary => "sanitary",
            _ => "unsanitary"
        };
    }
}