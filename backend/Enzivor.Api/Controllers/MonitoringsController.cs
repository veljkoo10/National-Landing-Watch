using Enzivor.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/monitorings")]
    public class MonitoringsController : ControllerBase
    {
        private readonly ILandfillSiteService _siteService;

        public MonitoringsController(ILandfillSiteService siteService)
        {
            _siteService = siteService;
        }

        [HttpGet("landfill/{landfillId:int}")]
        public async Task<IActionResult> GetMonitoringsByLandfill(int landfillId, CancellationToken ct)
        {
            var monitorings = await _siteService.GetMonitoringsAsync(landfillId, ct);
            return Ok(monitorings);
        }

        [HttpGet("landfill/{landfillId:int}/latest")]
        public async Task<IActionResult> GetLatestMonitoring(int landfillId, CancellationToken ct)
        {
            var latest = await _siteService.GetLatestMonitoringAsync(landfillId, ct);
            return Ok(latest);
        }
    }
}