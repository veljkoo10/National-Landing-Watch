using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/landfill-detections")]
    public class LandfillDetectionsController : ControllerBase
    {
        private readonly ILandfillProcessingService _processing;
        private readonly ILandfillDetectionRepository _detectionRepository;

        public LandfillDetectionsController(
            ILandfillProcessingService processing,
            ILandfillDetectionRepository detectionRepository)
        {
            _processing = processing;
            _detectionRepository = detectionRepository;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessLandfills(
           [FromBody] ProcessLandfillsRequest req,
           [FromQuery] bool persist = true,
           CancellationToken ct = default)
        {
            if (req is null) return BadRequest("Body is required.");
            if (string.IsNullOrWhiteSpace(req.CsvPath) || !System.IO.File.Exists(req.CsvPath))
                return BadRequest($"CSV file not found: {req.CsvPath}");

            if (req.ImageWidthPx <= 0 || req.ImageHeightPx <= 0)
                return BadRequest("ImageWidthPx and ImageHeightPx must be positive.");

            var results = _processing.ProcessLandfills(
                req.CsvPath,
                req.NorthWestLat,
                req.NorthWestLon,
                req.SouthEastLat,
                req.SouthEastLon,
                req.ImageWidthPx,
                req.ImageHeightPx
            );

            if (!persist)
                return Ok(new { persisted = 0, detections = results });

            var detectionsToSave = new List<LandfillDetection>(results.Count);

            foreach (var r in results)
            {
                if (!TryMapCategory(r.Type, out var cat))
                    return BadRequest($"Unknown category in CSV: '{r.Type}'.");

                detectionsToSave.Add(new LandfillDetection
                {
                    ImageName = r.ImageName,
                    Type = cat,
                    Confidence = r.Confidence,
                    SurfaceArea = r.SurfaceArea,
                    NorthWestLat = r.NorthWestLat,
                    NorthWestLon = r.NorthWestLon,
                    SouthEastLat = r.SouthEastLat,
                    SouthEastLon = r.SouthEastLon,
                    PolygonCoordinates = r.PolygonCoordinates,

                    RegionTag = r.RegionFromCsv,
                    Region = r.ParsedRegion
                });
            }

            try
            {
                await _detectionRepository.AddRangeAsync(detectionsToSave, ct);
                return Ok(new { persisted = detectionsToSave.Count, detections = results });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Request canceled by client.");
            }
            catch (Exception ex)
            {
                return Problem(statusCode: 500, title: "Failed to save detections", detail: ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDetections(
            [FromQuery] double? minConfidence = null,
            [FromQuery] LandfillCategory? type = null,
            CancellationToken ct = default)
        {
            var allDetections = await _detectionRepository.GetAllAsync(ct);

            var filtered = allDetections.AsQueryable();

            if (minConfidence.HasValue)
                filtered = filtered.Where(d => d.Confidence >= minConfidence.Value);

            if (type.HasValue)
                filtered = filtered.Where(d => d.Type == type.Value);

            return Ok(filtered.OrderByDescending(d => d.Confidence).ToList());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetectionById(int id, CancellationToken ct = default)
        {
            var detection = await _detectionRepository.GetByIdAsync(id, ct);

            if (detection == null)
                return NotFound($"Detection with ID {id} not found.");

            return Ok(detection);
        }

        private static bool TryMapCategory(string? raw, out LandfillCategory category)
        {
            category = raw?.Trim().ToLowerInvariant() switch
            {
                "illegal" => LandfillCategory.Illegal,
                "non_illegal" or "nonsanitary" or "public" => LandfillCategory.NonSanitary,
                "sanitary" => LandfillCategory.Sanitary,
                _ => (LandfillCategory)(-1)
            };
            return Enum.IsDefined(typeof(LandfillCategory), category);
        }
    }
}
