using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos.Landfills;
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

            if (detections.Count == 0)
                return 0;

            var now = DateTime.UtcNow;
            var currentYear = now.Year;
            var sites = new List<LandfillSite>(detections.Count);

            foreach (var d in detections)
            {
                // Clean landfill name
                var cleanName = (d.LandfillName ?? "")
                    .Trim()
                    .Trim('"', '“', '”')
                    .Replace("\"", string.Empty);

                var startYear = d.StartYear ?? (currentYear - 5);

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
                    StartYear = startYear,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                // Prepare DTO for emission calculations
                var dummyDto = new LandfillDto
                {
                    SurfaceArea = d.SurfaceArea,
                    ParsedRegion = d.Region,
                    Type = site.Category.ToString().ToLowerInvariant(),
                    StartYear = site.StartYear
                };

                _calculationService.CalculateMethaneEmissions(dummyDto);

                // Computed values
                site.EstimatedDepth = dummyDto.EstimatedDepth;
                site.EstimatedDensity = dummyDto.EstimatedDensity;
                site.EstimatedMSW = dummyDto.EstimatedMSW;
                site.MCF = dummyDto.MCF;
                site.EstimatedVolumeM3 = dummyDto.EstimatedVolume;
                site.EstimatedCH4Tons = dummyDto.CH4GeneratedTonnes;  // cumulative CH4
                site.EstimatedCO2eTons = dummyDto.CO2EquivalentTonnes; // cumulative CO2eq

                site.Detections.Add(d);
                sites.Add(site);
            }

            await _siteRepo.AddRangeAsync(sites, ct);
            await _siteRepo.SaveChangesAsync(ct);

            // Link detections to their corresponding sites
            foreach (var site in sites)
                foreach (var d in site.Detections)
                    d.LandfillSiteId = site.Id;

            await _siteRepo.SaveChangesAsync(ct);
            return sites.Count;
        }

        private static string? NormalizeKey(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return null;
            return v.Trim().ToLowerInvariant().Replace(" ", "")
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(ch => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch)
                             != System.Globalization.UnicodeCategory.NonSpacingMark)
                .Aggregate(new System.Text.StringBuilder(), (sb, ch) => sb.Append(ch))
                .ToString()
                .Normalize(System.Text.NormalizationForm.FormC)
                .Replace("đ", "dj");
        }
    }
}