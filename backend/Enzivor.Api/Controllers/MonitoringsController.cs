using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/monitorings")]
    public class MonitoringsController : ControllerBase
    {
        private readonly ILandfillSiteRepository _siteRepository;

        public MonitoringsController(ILandfillSiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        [HttpGet("landfill/{landfillId:int}")]
        public async Task<IActionResult> GetMonitoringsByLandfill(int landfillId, CancellationToken ct)
        {
            // IMPORTANT: _siteRepository.GetByIdAsync must include Detections (keep Include here)
            var site = await _siteRepository.GetByIdAsync(landfillId, ct);
            if (site is null) return NotFound();

            var year = DateTime.UtcNow.Year;
            var monitorings = site.Detections.Select(d => new MonitoringDto
            {
                Id = d.Id,
                LandfillId = landfillId,
                Year = year,
                AreaM2 = Math.Round(d.SurfaceArea, 2),
                VolumeM3 = Math.Round(d.SurfaceArea * 3, 2),
                WasteTons = Math.Round(d.SurfaceArea * 0.5, 2),
                Ch4Tons = Math.Round(d.SurfaceArea * 0.02, 2),
                Co2Tons = Math.Round(d.SurfaceArea * 0.05, 2)
            }).ToList();

            return Ok(monitorings);
        }

        [HttpGet("landfill/{landfillId:int}/latest")]
        public async Task<IActionResult> GetLatestMonitoring(int landfillId, CancellationToken ct)
        {
            var site = await _siteRepository.GetByIdAsync(landfillId, ct);
            if (site is null) return NotFound();

            var latest = site.Detections
                .OrderByDescending(d => d.Confidence)
                .FirstOrDefault();

            if (latest is null) return Ok(null);

            var year = DateTime.UtcNow.Year;
            var dto = new MonitoringDto
            {
                Id = latest.Id,
                LandfillId = landfillId,
                Year = year,
                AreaM2 = Math.Round(latest.SurfaceArea, 2),
                VolumeM3 = Math.Round(latest.SurfaceArea * 3, 2),
                WasteTons = Math.Round(latest.SurfaceArea * 0.5, 2),
                Ch4Tons = Math.Round(latest.SurfaceArea * 0.02, 2),
                Co2Tons = Math.Round(latest.SurfaceArea * 0.05, 2)
            };

            return Ok(dto);
        }
    }
}
