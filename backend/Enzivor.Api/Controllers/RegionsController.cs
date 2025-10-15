using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/regions")]
    public class RegionsController : ControllerBase
    {
        private readonly ILandfillSiteRepository _siteRepository;

        public RegionsController(ILandfillSiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableRegions(CancellationToken ct = default)
        {
            var regions = await _siteRepository.GetAvailableRegionsAsync(ct);

            return Ok(new
            {
                regions = regions,
                predefinedRegions = new[]
                {
                    "Vojvodina",
                    "Beograd",
                    "Sumadija i Pomoravlje",
                    "Zapadna Srbija",
                    "Istocna Srbija",
                    "Juzna Srbija"
                }
            });
        }

     
        [HttpGet("{regionTag}/sites")]
        public async Task<IActionResult> GetSitesByRegion(
            string regionTag,
            [FromQuery] LandfillCategory? category = null,
            [FromQuery] CurationStatus? status = null,
            [FromQuery] double? minAreaM2 = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(regionTag))
                return BadRequest("Region tag is required.");

            var sites = await _siteRepository.GetByRegionAsync(regionTag, ct);

            var filtered = sites.AsQueryable();

            if (category.HasValue)
                filtered = filtered.Where(s => s.Category == category.Value);

            if (status.HasValue)
                filtered = filtered.Where(s => s.Status == status.Value);

            if (minAreaM2.HasValue)
                filtered = filtered.Where(s => s.EstimatedAreaM2.HasValue && s.EstimatedAreaM2.Value >= minAreaM2.Value);

            var result = filtered.ToList();

            return Ok(new
            {
                region = regionTag,
                count = result.Count,
                sites = result
            });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetRegionsSummary(CancellationToken ct = default)
        {
            var allSites = await _siteRepository.GetAllAsync(ct);

            var summary = allSites
                .Where(s => !string.IsNullOrEmpty(s.RegionTag))
                .GroupBy(s => s.RegionTag)
                .Select(g => new
                {
                    Region = g.Key,
                    Count = g.Count(),
                    TotalAreaM2 = g.Sum(s => s.EstimatedAreaM2 ?? 0),
                    ByCategory = g.GroupBy(s => s.Category)
                        .ToDictionary(cg => cg.Key.ToString(), cg => cg.Count()),
                    ByStatus = g.GroupBy(s => s.Status)
                        .ToDictionary(sg => sg.Key.ToString(), sg => sg.Count())
                })
                .OrderByDescending(r => r.Count)
                .ToList();

            return Ok(summary);
        }
    }
}
