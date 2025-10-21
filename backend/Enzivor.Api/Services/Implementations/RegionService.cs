using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;
using Enzivor.Api.Models.Static;
using System.Globalization;
using System.Text;

namespace Enzivor.Api.Services.Implementations
{
    public class RegionService : IRegionService
    {
        private readonly ILandfillSiteRepository _siteRepository;

        public RegionService(ILandfillSiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        public async Task<List<RegionDto>> GetAllRegionsAsync(CancellationToken ct = default)
        {
            var allSites = await _siteRepository.GetAllAsync(ct);

            return RegionDefinitions.All.Select(def =>
            {
                var sites = allSites.Where(s => Normalize(s.RegionTag) == def.Key).ToList();

                var ch4 = Math.Round(sites.Sum(s => s.EstimatedCH4TonsPerYear ?? 0), 2);
                var co2 = Math.Round(sites.Sum(s => s.EstimatedCO2eTonsPerYear ?? 0), 2);
                var waste = Math.Round(sites.Sum(s => s.EstimatedMSW ?? 0), 2);

                return new RegionDto
                {
                    Id = def.Id,
                    Name = def.Name,
                    Population = def.Population,
                    AreaKm2 = def.AreaKm2,
                    LandfillCount = sites.Count,
                    Ch4Tons = ch4,
                    Co2Tons = co2,
                    TotalWaste = waste,
                    PollutionLevel = GetPollutionLevel(ch4, def.AreaKm2)
                };
            }).ToList();
        }

        public async Task<RegionDto?> GetRegionByIdAsync(int id, CancellationToken ct = default)
        {
            var def = RegionDefinitions.All.FirstOrDefault(r => r.Id == id);
            if (def is null) return null;

            var allSites = await _siteRepository.GetAllAsync(ct);
            var sites = allSites.Where(s => Normalize(s.RegionTag) == def.Key).ToList();

            var ch4 = Math.Round(sites.Sum(s => s.EstimatedCH4TonsPerYear ?? 0), 2);
            var co2 = Math.Round(sites.Sum(s => s.EstimatedCO2eTonsPerYear ?? 0), 2);
            var waste = Math.Round(sites.Sum(s => s.EstimatedMSW ?? 0), 2);

            return new RegionDto
            {
                Id = def.Id,
                Name = def.Name,
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
            var def = RegionDefinitions.GetByKey(key ?? "");
            if (def is null) return null;

            var allSites = await _siteRepository.GetAllAsync(ct);
            var sites = allSites.Where(s => Normalize(s.RegionTag) == def.Key).ToList();

            var ch4 = Math.Round(sites.Sum(s => s.EstimatedCH4TonsPerYear ?? 0), 2);
            var co2 = Math.Round(sites.Sum(s => s.EstimatedCO2eTonsPerYear ?? 0), 2);
            var waste = Math.Round(sites.Sum(s => s.EstimatedMSW ?? 0), 2);

            return new RegionDto
            {
                Id = def.Id,
                Name = def.Name,
                Population = def.Population,
                AreaKm2 = def.AreaKm2,
                LandfillCount = sites.Count,
                Ch4Tons = ch4,
                Co2Tons = co2,
                TotalWaste = waste,
                PollutionLevel = GetPollutionLevel(ch4, def.AreaKm2)
            };
        }

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
                "kosovoimetohija" or "kosovoandmetohija" => "kosovoimetohija",
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