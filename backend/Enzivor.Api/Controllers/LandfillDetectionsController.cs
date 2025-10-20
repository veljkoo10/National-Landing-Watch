using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/admin/detections")]
    public class LandfillDetectionsController : ControllerBase
    {
        private readonly ILandfillDetectionService _service;

        public LandfillDetectionsController(ILandfillDetectionService service)
        {
            _service = service;
        }

        [HttpPost("process-production")]
        public async Task<IActionResult> ProcessProduction([FromBody] ProcessProductionRequest req, CancellationToken ct = default)
        {
            if (req is null) return BadRequest("Request required");

            try
            {
                var summary = await _service.ProcessProductionAsync(req, ct);
                return Ok(summary);
            }
            catch (FileNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(CancellationToken ct = default)
        {
            var stats = await _service.GetStatsAsync(ct);
            return Ok(stats);
        }
    }
}