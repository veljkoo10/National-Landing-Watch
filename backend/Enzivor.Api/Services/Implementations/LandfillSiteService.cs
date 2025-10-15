using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    public sealed class LandfillSiteService : ILandfillSiteService
    {
        private readonly ILandfillDetectionRepository _detRepo;
        private readonly ILandfillSiteRepository _siteRepo;

        public LandfillSiteService(
            ILandfillDetectionRepository detRepo,
            ILandfillSiteRepository siteRepo)
        {
            _detRepo = detRepo;
            _siteRepo = siteRepo;
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
                    Name = null,
                    Category = d.Type,
                    Status = CurationStatus.Pending,

                    PointLat = (d.NorthWestLat + d.SouthEastLat) / 2.0,
                    PointLon = (d.NorthWestLon + d.SouthEastLon) / 2.0,

                    BoundaryGeoJson = d.PolygonCoordinates,
                    EstimatedAreaM2 = d.SurfaceArea,

                    RegionTag = d.RegionTag,
                    Region = d.Region,

                    CreatedAt = now,
                    UpdatedAt = now
                };

                site.Detections.Add(d);
                sites.Add(site);
            }

            await _siteRepo.AddRangeAsync(sites, ct);
            await _siteRepo.SaveChangesAsync(ct); 

            return sites.Count;
        }
    }
}
