using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/landfills")]
    public class LandfillsController : ControllerBase
    {
        private readonly ILandfillQueryService _queryService;
        private readonly ILandfillSiteService _siteService;

        public LandfillsController(
            ILandfillQueryService queryService,
            ILandfillSiteService siteService)
        {
            _queryService = queryService;
            _siteService = siteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShowLandfillDto>>> GetAllLandfills(CancellationToken ct)
        {
            var result = await _queryService.GetAllLandfillsAsync(ct);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ShowLandfillDto>> GetLandfillById(int id, CancellationToken ct)
        {
            var landfill = await _queryService.GetLandfillByIdAsync(id, ct);
            return landfill is null ? NotFound() : Ok(landfill);
        }

        [HttpGet("region/{regionKey}")]
        public async Task<ActionResult<IEnumerable<ShowLandfillDto>>> GetLandfillsByRegion(string regionKey, CancellationToken ct)
        {
            var result = await _queryService.GetLandfillsByRegionAsync(regionKey, ct);
            return Ok(result);
        }

        [HttpPost("promote")]
        public async Task<IActionResult> PromoteDetectionsToSites(
            [FromQuery] double? minConfidence = null,
            [FromQuery] LandfillCategory? category = null,
            CancellationToken ct = default)
        {
            var createdCount = await _siteService.CreateSitesFromUnlinkedDetectionsAsync(minConfidence, category, ct);

            if (createdCount == 0)
                return Ok(new
                {
                    created = 0,
                    message = "No new sites were created. All detections are already linked."
                });

            return Ok(new
            {
                created = createdCount,
                message = $"{createdCount} new landfill sites were created."
            });
        }
    }
}