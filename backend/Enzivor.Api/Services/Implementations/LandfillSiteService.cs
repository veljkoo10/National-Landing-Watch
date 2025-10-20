using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    /// <summary>
    /// Handles creation and update logic for landfill sites.
    /// </summary>
    public sealed class LandfillSiteService : ILandfillSiteService
    {
        private readonly ILandfillDetectionRepository _detRepo;
        private readonly ILandfillSiteRepository _siteRepo;
        private readonly IMethaneCalculationService _methaneService;

        public LandfillSiteService(
            ILandfillDetectionRepository detRepo,
            ILandfillSiteRepository siteRepo,
            IMethaneCalculationService methaneService)
        {
            _detRepo = detRepo;
            _siteRepo = siteRepo;
            _methaneService = methaneService;
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
                var site = new LandfillSite
                {
                    Name = d.LandfillName,
                    Category = d.Type,
                    Status = CurationStatus.Pending,

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
                    ParsedRegion = d.Region
                };
                _methaneService.CalculateMethaneEmissions(dummyDto);

                site.EstimatedDepth = dummyDto.EstimatedDepth;
                site.EstimatedDensity = dummyDto.EstimatedDensity;
                site.EstimatedMSW = dummyDto.EstimatedMSW;
                site.MCF = dummyDto.MCF;
                site.EstimatedCH4TonsPerYear = dummyDto.CH4GeneratedTonnesPerYear;
                site.EstimatedCO2eTonsPerYear = dummyDto.CO2EquivalentTonnesPerYear;

                site.Detections.Add(d);
                sites.Add(site);
            }

            // Insert sites and get IDs
            await _siteRepo.AddRangeAsync(sites, ct);
            await _siteRepo.SaveChangesAsync(ct);

            // Explicitly set FK to ensure it is persisted
            foreach (var site in sites)
            {
                foreach (var d in site.Detections)
                {
                    d.LandfillSiteId = site.Id;
                }
            }

            // Persist FK updates
            await _siteRepo.SaveChangesAsync(ct);

            return sites.Count;
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
