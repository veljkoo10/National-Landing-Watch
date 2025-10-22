using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos.Statistics;
using Enzivor.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("total-waste-by-region")]
        public async Task<ActionResult<IEnumerable<WasteByRegionDto>>> GetTotalWasteByRegion(CancellationToken ct = default)
        {
            var result = await _statisticsService.GetTotalWasteByRegionAsync(ct);
            return Ok(result);
        }

        [HttpGet("landfill-types")]
        public async Task<ActionResult<IEnumerable<LandfillSite>>> GetLandfillTypes(CancellationToken ct = default)
        {
            var result = await _statisticsService.GetLandfillTypesAsync(ct);
            return Ok(result);
        }

        [HttpGet("ch4-over-time")]
        public async Task<ActionResult<Ch4OverTimeDto>> GetCh4OverTime(CancellationToken ct = default)
        {
            var result = await _statisticsService.GetCh4OverTimeAsync(ct);
            return Ok(result);
        }

        [HttpGet("top3-largest")]
        public async Task<ActionResult<IEnumerable<LandfillSite>>> GetTop3LargestLandfills(CancellationToken ct = default)
        {
            var result = await _statisticsService.GetTopLargestLandfillsAsync(3, ct);
            return Ok(result);
        }

        [HttpGet("most-impacted")]
        public async Task<ActionResult<MostImpactedRegionFullDto>> GetMostImpactedRegion(CancellationToken ct = default)
        {
            var result = await _statisticsService.GetMostImpactedRegionAsync(ct);
            return Ok(result);
        }

        [HttpGet("landfill-growth")]
        public async Task<ActionResult<IEnumerable<GrowthRowDto>>> GetLandfillGrowth(CancellationToken ct = default)
        {
            var result = await _statisticsService.GetLandfillGrowthAsync(ct);
            return Ok(result);
        }

        [HttpGet("emissions-per-capita")]
        public async Task<ActionResult<IEnumerable<EmissionPerCapitaDto>>> GetEmissionsPerCapita(CancellationToken ct = default)
        {
            var result = await _statisticsService.GetEmissionsPerCapitaAsync(ct);
            return Ok(result);
        }
    }
}