using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    /// <summary>
    /// Handles creation, monitoring, and statistics logic for landfill sites.
    /// </summary>
    public sealed class LandfillSiteService : ILandfillSiteService
    {
        private readonly ILandfillDetectionRepository _detRepo;
        private readonly ILandfillSiteRepository _siteRepo;
        private readonly ICalculationService _calculationService;

        public LandfillSiteService(
            ILandfillDetectionRepository detRepo,
            ILandfillSiteRepository siteRepo,
            ICalculationService calculationService)
        {
            _detRepo = detRepo;
            _siteRepo = siteRepo;
            _calculationService = calculationService;
        }

        public async Task<int> CreateSitesFromUnlinkedDetectionsAsync(
    double? minConfidence = null,
    LandfillCategory? category = null,
    CancellationToken ct = default)
        {
            var detections = await _detRepo.GetUnlinkedAsync(ct);
            if (minConfidence.HasValue)
                detections = detections.Where(d => d.Confidence >= minConfidence.Value).ToList();

            if (category.HasValue)
                detections = detections.Where(d => d.Type == category.Value).ToList();

            if (detections.Count == 0) return 0;

            var now = DateTime.UtcNow;
            var sites = new List<LandfillSite>(detections.Count);

            foreach (var d in detections)
            {
                // Remove extra quotes and whitespace
                var cleanName = (d.LandfillName ?? "")
                    .Trim()
                    .Trim('"')
                    .Trim('“', '”')
                    .Replace("\"", string.Empty);

                var site = new LandfillSite
                {
                    Name = string.IsNullOrWhiteSpace(cleanName) ? $"Landfill-{Guid.NewGuid():N}" : cleanName,
                    Category = d.Type,
                    PointLat = (d.NorthWestLat + d.SouthEastLat) / 2.0,
                    PointLon = (d.NorthWestLon + d.SouthEastLon) / 2.0,
                    BoundaryGeoJson = d.PolygonCoordinates,
                    EstimatedAreaM2 = d.SurfaceArea,
                    RegionTag = NormalizeKey(d.RegionTag),
                    Region = d.Region,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                var dummyDto = new LandfillDto
                {
                    SurfaceArea = d.SurfaceArea,
                    ParsedRegion = d.Region,
                    Type = site.Category.ToString().ToLowerInvariant()
                };

                _calculationService.CalculateMethaneEmissions(dummyDto);

                // Computed values
                site.EstimatedDepth = dummyDto.EstimatedDepth;
                site.EstimatedDensity = dummyDto.EstimatedDensity;
                site.EstimatedMSW = dummyDto.EstimatedMSW;
                site.MCF = dummyDto.MCF;
                site.EstimatedVolumeM3 = dummyDto.EstimatedVolume;
                site.EstimatedCH4TonsPerYear = dummyDto.CH4GeneratedTonnesPerYear;
                site.EstimatedCO2eTonsPerYear = dummyDto.CO2EquivalentTonnesPerYear;

                site.Detections.Add(d);
                sites.Add(site);
            }

            await _siteRepo.AddRangeAsync(sites, ct);
            await _siteRepo.SaveChangesAsync(ct);

            foreach (var site in sites)
                foreach (var d in site.Detections)
                    d.LandfillSiteId = site.Id;

            await _siteRepo.SaveChangesAsync(ct);
            return sites.Count;
        }

        public async Task<IEnumerable<MonitoringDto>> GetMonitoringsAsync(int landfillId, CancellationToken ct = default)
        {
            var site = await _siteRepo.GetByIdWithDetectionsAsync(landfillId, ct);
            if (site is null || site.Detections == null || !site.Detections.Any())
                return Enumerable.Empty<MonitoringDto>();

            var year = DateTime.UtcNow.Year;
            var dtos = site.Detections.Select(d => new MonitoringDto
            {
                Id = d.Id,
                LandfillId = landfillId,
                Year = year,
                AreaM2 = Math.Round(d.SurfaceArea, 2)
            }).ToList();

            foreach (var dto in dtos)
            {
                var landfillDto = new LandfillDto
                {
                    SurfaceArea = dto.AreaM2,
                    ParsedRegion = site.Region,
                    Type = site.Category.ToString().ToLowerInvariant()
                };

                _calculationService.CalculateMethaneEmissions(landfillDto);

                dto.VolumeM3 = landfillDto.EstimatedVolume;
                dto.WasteTons = landfillDto.EstimatedMSW;
                dto.Ch4Tons = landfillDto.CH4GeneratedTonnesPerYear;
                dto.Co2Tons = landfillDto.CO2EquivalentTonnesPerYear;
            }

            return dtos;
        }

        public async Task<MonitoringDto?> GetLatestMonitoringAsync(int landfillId, CancellationToken ct = default)
        {
            var site = await _siteRepo.GetByIdWithDetectionsAsync(landfillId, ct);
            if (site is null || site.Detections == null || !site.Detections.Any())
                return null;

            var latest = site.Detections.OrderByDescending(d => d.Confidence).FirstOrDefault();
            if (latest == null)
                return null;

            var landfillDto = new LandfillDto
            {
                SurfaceArea = latest.SurfaceArea,
                ParsedRegion = site.Region,
                Type = site.Category.ToString().ToLowerInvariant()
            };

            _calculationService.CalculateMethaneEmissions(landfillDto);

            return new MonitoringDto
            {
                Id = latest.Id,
                LandfillId = landfillId,
                Year = DateTime.UtcNow.Year,
                AreaM2 = landfillDto.SurfaceArea,
                VolumeM3 = landfillDto.EstimatedVolume,
                WasteTons = landfillDto.EstimatedMSW,
                Ch4Tons = landfillDto.CH4GeneratedTonnesPerYear,
                Co2Tons = landfillDto.CO2EquivalentTonnesPerYear
            };
        }

        private static string? NormalizeKey(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return null;
            return v.Trim().ToLowerInvariant().Replace(" ", "")
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(ch => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .Aggregate(new System.Text.StringBuilder(), (sb, ch) => sb.Append(ch))
                .ToString()
                .Normalize(System.Text.NormalizationForm.FormC)
                .Replace("đ", "dj");
        }
    }
}