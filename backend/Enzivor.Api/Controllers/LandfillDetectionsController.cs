using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/admin/detections")]
    public class LandfillDetectionsController : ControllerBase
    {
        private readonly IProductionLandfillProcessor _processor;
        private readonly ILandfillDetectionRepository _detectionRepository;

        public LandfillDetectionsController(IProductionLandfillProcessor processor, ILandfillDetectionRepository detectionRepository)
        {
            _processor = processor;
            _detectionRepository = detectionRepository;
        }

        [HttpPost("process-production")]
        public async Task<IActionResult> ProcessProduction([FromBody] ProcessProductionRequest req, CancellationToken ct = default)
        {
            if (req is null) return BadRequest("Request required");

            if (!System.IO.File.Exists(req.ClassificationCsvPath)) return BadRequest("Classification CSV not found");
            if (!System.IO.File.Exists(req.SegmentationCsvPath)) return BadRequest("Segmentation CSV not found");
            if (!System.IO.File.Exists(req.MetadataSpreadsheetPath)) return BadRequest("Metadata file not found");

            var results = await _processor.ProcessProductionData(req.ClassificationCsvPath, req.SegmentationCsvPath, req.MetadataSpreadsheetPath);

            var detections = results.Select(r => new LandfillDetection
            {
                ImageName = r.ImageName,
                LandfillName = r.KnownLandfillName,
                Type = MapCategory(r.Type),
                Confidence = r.Confidence,
                SurfaceArea = r.SurfaceArea,
                NorthWestLat = r.NorthWestLat,
                NorthWestLon = r.NorthWestLon,
                SouthEastLat = r.SouthEastLat,
                SouthEastLon = r.SouthEastLon,
                PolygonCoordinates = r.PolygonCoordinates,
                RegionTag = r.RegionTag,
                Region = r.ParsedRegion
            }).ToList();

            await _detectionRepository.AddRangeAsync(detections, ct);

            return Ok(new
            {
                processed = results.Count,
                persisted = detections.Count,
                withSegmentation = results.Count(r => !string.IsNullOrEmpty(r.PolygonCoordinates)),
                withMetadata = results.Count(r => !string.IsNullOrEmpty(r.KnownLandfillName))
            });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(CancellationToken ct = default)
        {
            var all = await _detectionRepository.GetAllAsync(ct);
            var unlinked = await _detectionRepository.GetUnlinkedAsync(ct);

            return Ok(new
            {
                Total = all.Count,
                Linked = all.Count - unlinked.Count,
                Unlinked = unlinked.Count,
                ReadyForPromotion = unlinked.Count(d => d.Confidence >= 0.8),
                AvgConfidence = all.Any() ? all.Average(d => d.Confidence) : 0
            });
        }

        private LandfillCategory MapCategory(string type) => type?.ToLowerInvariant() switch
        {
            "illegal" => LandfillCategory.Illegal,
            "non_illegal" or "nonsanitary" => LandfillCategory.NonSanitary,
            "sanitary" => LandfillCategory.Sanitary,
            _ => LandfillCategory.Illegal
        };
    }
}
