using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace Enzivor.Api.Services.Implementations
{
    public class RegionStatisticsService : IRegionStatisticsService
    {
        private readonly ILandfillSiteRepository _siteRepository;

        private sealed record RegionDef(int Id, string Key, string NameSr, int Population, int AreaKm2);

        private static readonly List<RegionDef> Regions = new()
        {
            new(1, "vojvodina",           "Vojvodina",              1_900_000, 21_506),
            new(2, "beograd",             "Beograd",                1_675_000, 3_226),
            new(3, "zapadnasrbija",       "Zapadna Srbija",         1_200_000, 26_000),
            new(4, "sumadijaipomoravlje", "Šumadija i Pomoravlje",  1_000_000, 13_000),
            new(5, "istocnasrbija",       "Istočna Srbija",           800_000, 19_000),
            new(6, "juznasrbija",         "Južna Srbija",             900_000, 15_000),
        };

        public RegionStatisticsService(ILandfillSiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        public async Task<List<RegionDto>> GetAllRegionsAsync(CancellationToken ct = default)
        {
            var allSites = await _siteRepository.GetAllAsync(ct);

            return Regions.Select(r =>
            {
                var sites = allSites.Where(s => Normalize(s.RegionTag) == r.Key).ToList();

                var ch4 = Math.Round(sites.Sum(s => s.EstimatedCH4TonsPerYear ?? 0), 2);
                var co2 = Math.Round(sites.Sum(s => s.EstimatedCO2eTonsPerYear ?? 0), 2);
                var waste = Math.Round(sites.Sum(s => s.EstimatedMSW ?? 0), 2);

                return new RegionDto
                {
                    Id = r.Id,
                    Name = r.NameSr,
                    Population = r.Population,
                    AreaKm2 = r.AreaKm2,
                    LandfillCount = sites.Count,
                    Ch4Tons = ch4,
                    Co2Tons = co2,
                    TotalWaste = waste,
                    PollutionLevel = GetPollutionLevel(ch4, r.AreaKm2)
                };
            }).ToList();
        }

        public async Task<RegionDto?> GetRegionByIdAsync(int id, CancellationToken ct = default)
        {
            var def = Regions.FirstOrDefault(r => r.Id == id);
            if (def is null) return null;

            // ✅ Use the same in-memory aggregation as other methods
            var allSites = await _siteRepository.GetAllAsync(ct);
            var sites = allSites.Where(s => Normalize(s.RegionTag) == def.Key).ToList();

            var ch4 = Math.Round(sites.Sum(s => s.EstimatedCH4TonsPerYear ?? 0), 2);
            var co2 = Math.Round(sites.Sum(s => s.EstimatedCO2eTonsPerYear ?? 0), 2);
            var waste = Math.Round(sites.Sum(s => s.EstimatedMSW ?? 0), 2);

            return new RegionDto
            {
                Id = def.Id,
                Name = def.NameSr,
                Population = def.Population,
                AreaKm2 = def.AreaKm2,
                LandfillCount = sites.Count,
                Ch4Tons = ch4,
                Co2Tons = co2,
                TotalWaste = waste,
                PollutionLevel = GetPollutionLevel(ch4, def.AreaKm2)
            };
        }

        public async Task<RegionDto?> GetRegionByNameAsync(string name, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var key = MapInputToKey(name);
            if (key is null) return null;

            var def = Regions.FirstOrDefault(r => r.Key == key);
            if (def is null) return null; // safety if map returns a key you didn't define

            var allSites = await _siteRepository.GetAllAsync(ct);
            var sites = allSites.Where(s => Normalize(s.RegionTag) == key).ToList();

            var ch4 = Math.Round(sites.Sum(s => s.EstimatedCH4TonsPerYear ?? 0), 2);
            var co2 = Math.Round(sites.Sum(s => s.EstimatedCO2eTonsPerYear ?? 0), 2);
            var waste = Math.Round(sites.Sum(s => s.EstimatedMSW ?? 0), 2);

            return new RegionDto
            {
                Id = def.Id,
                Name = def.NameSr,
                Population = def.Population,
                AreaKm2 = def.AreaKm2,
                LandfillCount = sites.Count,
                Ch4Tons = ch4,
                Co2Tons = co2,
                TotalWaste = waste,
                PollutionLevel = GetPollutionLevel(ch4, def.AreaKm2)
            };
        }

        // ---------- Helpers ----------

        private static string? MapInputToKey(string input)
        {
            var k = Normalize(input);
            return k switch
            {
                "vojvodina" => "vojvodina",
                "beograd" or "belgrade" => "beograd",
                "zapadnasrbija" or "westernserbia" => "zapadnasrbija",
                "sumadijaipomoravlje" or "sumadijaandpomoravlje" => "sumadijaipomoravlje",
                "istocnasrbija" or "easternserbia" => "istocnasrbija",
                "juznasrbija" or "southernserbia" => "juznasrbija",
                // if you later add:
                // "kosovoimetohija" or "kosovoandmetohija" => "kosovoimetohija",
                _ => null
            };
        }

        private static string Normalize(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            var lower = s.Trim().ToLowerInvariant();
            var noSpaces = new string(lower.Where(c => !char.IsWhiteSpace(c)).ToArray());

            var formD = noSpaces.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (var ch in formD)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);

            return sb.ToString().Normalize(NormalizationForm.FormC)
                     .Replace("đ", "dj");
        }

        private static string GetPollutionLevel(double ch4TonsPerYear, int areaKm2)
        {
            if (areaKm2 <= 0) return "unknown";
            var density = ch4TonsPerYear / areaKm2;
            return density switch { > 2.0 => "high", > 0.5 => "medium", _ => "low" };
        }
    }
}
