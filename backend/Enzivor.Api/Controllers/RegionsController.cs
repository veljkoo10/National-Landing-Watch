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
        private readonly IRegionService _regionService;

        public RegionsController(IRegionService regionStatisticsService)
        {
            _regionService = regionStatisticsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRegions(CancellationToken ct = default)
        {
            var regions = await _regionService.GetAllRegionsAsync(ct);
            return Ok(regions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRegionById(int id, CancellationToken ct = default)
        {
            var region = await _regionService.GetRegionByIdAsync(id, ct);
            if (region == null) return NotFound();
            return Ok(region);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetRegionByName(string name, CancellationToken ct = default)
        {
            var region = await _regionService.GetRegionByNameAsync(name, ct); 
            if (region == null) return NotFound();
            return Ok(region);
        }
    }
}