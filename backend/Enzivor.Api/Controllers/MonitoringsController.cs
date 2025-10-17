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

        [HttpGet("landfill/{landfillId}")]
        public async Task<IActionResult> GetMonitoringsByLandfill(int landfillId)
        {
            var site = await _siteRepository.GetByIdAsync(landfillId, default);
            if (site == null)
                return NotFound();

            // Convert detections to monitoring data format expected by frontend
            var monitorings = site.Detections.Select(d => new
            {
                //Id = d.Id,
                LandfillId = landfillId,
                Year = 2024,
                AreaM2 = Math.Round(d.SurfaceArea, 2),
                VolumeM3 = Math.Round(d.SurfaceArea * 3, 2),
                WasteTons = Math.Round(d.SurfaceArea * 0.5, 2),
                Ch4Tons = Math.Round(d.SurfaceArea * 0.02, 2),
                Co2Tons = Math.Round(d.SurfaceArea * 0.05, 2)
            }).ToList();

            return Ok(monitorings);
        }

        [HttpGet("landfill/{landfillId}/latest")]
        public async Task<IActionResult> GetLatestMonitoring(int landfillId)
        {
            var site = await _siteRepository.GetByIdAsync(landfillId, default);
            if (site == null)
                return NotFound();

            var latestDetection = site.Detections
                .OrderByDescending(d => d.Confidence)
                .FirstOrDefault();

            if (latestDetection == null)
                return Ok(null);

            var monitoring = new
            {
                Id = latestDetection.Id,
                LandfillId = landfillId,
                Year = 2024,
                AreaM2 = Math.Round(latestDetection.SurfaceArea, 2),
                VolumeM3 = Math.Round(latestDetection.SurfaceArea * 3, 2),
                WasteTons = Math.Round(latestDetection.SurfaceArea * 0.5, 2),
                Ch4Tons = Math.Round(latestDetection.SurfaceArea * 0.02, 2),
                Co2Tons = Math.Round(latestDetection.SurfaceArea * 0.05, 2)
            };

            return Ok(monitoring);
        }
    }
}


