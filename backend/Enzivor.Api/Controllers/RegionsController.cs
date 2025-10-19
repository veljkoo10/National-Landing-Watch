using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/regions")]
    public class RegionsController : ControllerBase
    {
        private readonly IRegionStatisticsService _regionStatisticsService;

        public RegionsController(IRegionStatisticsService regionStatisticsService)
        {
            _regionStatisticsService = regionStatisticsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRegions(CancellationToken ct = default)
        {
            var regions = await _regionStatisticsService.GetAllRegionsAsync(ct);
            return Ok(regions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRegionById(int id, CancellationToken ct = default)
        {
            var region = await _regionStatisticsService.GetRegionByIdAsync(id, ct);
            if (region == null) return NotFound();
            return Ok(region);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetRegionByName(string name, CancellationToken ct = default)
        {
            var region = await _regionStatisticsService.GetRegionByNameAsync(name, ct); 
            if (region == null) return NotFound();
            return Ok(region);
        }
    }
}