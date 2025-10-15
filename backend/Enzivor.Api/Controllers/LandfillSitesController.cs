using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/landfill-sites")]
    public class LandfillSitesController : ControllerBase
    {
        private readonly ILandfillSiteService _siteService;
        private readonly ILandfillSiteRepository _siteRepository;

        public LandfillSitesController(
            ILandfillSiteService siteService,
            ILandfillSiteRepository siteRepository)
        {
            _siteService = siteService;
            _siteRepository = siteRepository;
        }

    
        [HttpGet]
        public async Task<IActionResult> GetSites(
            [FromQuery] LandfillCategory? category = null,
            [FromQuery] CurationStatus? status = null,
            [FromQuery] double? minAreaM2 = null,
            [FromQuery] string? municipality = null,
            [FromQuery] string? regionTag = null,
            CancellationToken ct = default)
        {
            var sites = await _siteRepository.GetAllAsync(ct);

            var q = sites.AsQueryable();

            if (category.HasValue)
                q = q.Where(s => s.Category == category.Value);

            if (status.HasValue)
                q = q.Where(s => s.Status == status.Value);

            if (minAreaM2.HasValue)
                q = q.Where(s => s.EstimatedAreaM2.HasValue && s.EstimatedAreaM2.Value >= minAreaM2.Value);

            if (!string.IsNullOrWhiteSpace(municipality))
                q = q.Where(s => s.Municipality != null && s.Municipality.Contains(municipality, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(regionTag))
                q = q.Where(s => s.RegionTag != null && s.RegionTag.Equals(regionTag, StringComparison.OrdinalIgnoreCase));

            var result = q.OrderByDescending(s => s.EstimatedAreaM2 ?? 0).ToList();

            return Ok(result);
        }

   
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSiteById(int id, CancellationToken ct = default)
        {
            var site = await _siteRepository.GetByIdAsync(id, ct);

            if (site == null)
                return NotFound($"Landfill site with ID {id} not found.");

            return Ok(site);
        }

        [HttpGet("{id}/with-detections")]
        public async Task<IActionResult> GetSiteWithDetections(int id, CancellationToken ct = default)
        {
            var site = await _siteRepository.GetByIdWithDetectionsAsync(id, ct);

            if (site == null)
                return NotFound($"Landfill site with ID {id} not found.");

            return Ok(new
            {
                Site = site,
                DetectionCount = site.Detections.Count,
                AverageConfidence = site.Detections.Any() ? site.Detections.Average(d => d.Confidence) : 0,
                Detections = site.Detections.OrderByDescending(d => d.Confidence)
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSite(int id, [FromBody] UpdateSiteRequest request, CancellationToken ct = default)
        {
            var site = await _siteRepository.GetByIdAsync(id, ct);

            if (site == null)
                return NotFound($"Landfill site with ID {id} not found.");

            site.Name = request.Name;
            site.Status = request.Status;
            site.Municipality = request.Municipality;
            site.RegionTag = request.RegionTag;
            site.EstimatedAreaM2 = request.EstimatedAreaM2;
            site.EstimatedVolumeM3 = request.EstimatedVolumeM3;
            site.EstimatedCH4TonsPerYear = request.EstimatedCH4TonsPerYear;
            site.EstimatedCO2eTonsPerYear = request.EstimatedCO2eTonsPerYear;
            site.StartYear = request.StartYear;
            site.AnnualMSWTons = request.AnnualMSWTons;
            site.UpdatedAt = DateTime.UtcNow;

            await _siteRepository.SaveChangesAsync(ct);

            return Ok(site);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSite(int id, CancellationToken ct = default)
        {
            var site = await _siteRepository.GetByIdAsync(id, ct);

            if (site == null)
                return NotFound($"Landfill site with ID {id} not found.");

            await _siteRepository.DeleteAsync(site, ct);
            await _siteRepository.SaveChangesAsync(ct);

            return NoContent();
        }

        [HttpPost("promote")]
        public async Task<IActionResult> PromoteDetectionsToSites(
            [FromQuery] double? minConfidence = null,
            [FromQuery] LandfillCategory? category = null,
            CancellationToken ct = default)
        {
            var createdCount = await _siteService.CreateSitesFromUnlinkedDetectionsAsync(
                minConfidence, category, ct
            );

            if (createdCount == 0)
                return Ok(new { created = 0, message = "No new sites were created. All detections are already linked." });

            return Ok(new { created = createdCount, message = $"{createdCount} new landfill sites were created." });
        }
    }
}
