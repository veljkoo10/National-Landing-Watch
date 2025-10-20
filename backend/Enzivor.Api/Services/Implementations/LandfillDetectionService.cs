using AutoMapper;
using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    public sealed class LandfillDetectionService : ILandfillDetectionService
    {
        private readonly IProductionLandfillProcessor _processor;
        private readonly ILandfillDetectionRepository _repo;
        private readonly IMapper _mapper;

        public LandfillDetectionService(
            IProductionLandfillProcessor processor,
            ILandfillDetectionRepository repo,
            IMapper mapper)
        {
            _processor = processor;
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ProductionProcessResultDto> ProcessProductionAsync(ProcessProductionRequest req, CancellationToken ct = default)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (!File.Exists(req.ClassificationCsvPath)) throw new FileNotFoundException("Classification CSV not found", req.ClassificationCsvPath);
            if (!File.Exists(req.SegmentationCsvPath)) throw new FileNotFoundException("Segmentation CSV not found", req.SegmentationCsvPath);
            if (!File.Exists(req.MetadataSpreadsheetPath)) throw new FileNotFoundException("Metadata file not found", req.MetadataSpreadsheetPath);

            var results = await _processor.ProcessProductionData(
                req.ClassificationCsvPath,
                req.SegmentationCsvPath,
                req.MetadataSpreadsheetPath);

            var detections = _mapper.Map<List<LandfillDetection>>(results);

            foreach (var (entity, dto) in detections.Zip(results))
                entity.Type = MapCategory(dto.Type);

            await _repo.AddRangeAsync(detections, ct);

            return new ProductionProcessResultDto
            {
                Processed = results.Count,
                Persisted = detections.Count,
                WithSegmentation = results.Count(r => !string.IsNullOrEmpty(r.PolygonCoordinates)),
                WithMetadata = results.Count(r => !string.IsNullOrEmpty(r.KnownLandfillName))
            };
        }

        public async Task<DetectionStatsDto> GetStatsAsync(CancellationToken ct = default)
        {
            var all = await _repo.GetAllAsync(ct);
            var unlinked = await _repo.GetUnlinkedAsync(ct);

            var linked = all.Count - unlinked.Count;
            var ready = unlinked.Count(d => d.Confidence >= 0.8);
            var avg = all.Any() ? all.Average(d => d.Confidence) : 0.0;

            return new DetectionStatsDto
            {
                Total = all.Count,
                Linked = linked,
                Unlinked = unlinked.Count,
                ReadyForPromotion = ready,
                AvgConfidence = avg
            };
        }

        private static LandfillCategory MapCategory(string type) => type?.ToLowerInvariant() switch
        {
            "illegal" => LandfillCategory.Illegal,
            "non_illegal" or "nonsanitary" => LandfillCategory.NonSanitary,
            "sanitary" => LandfillCategory.Sanitary,
            _ => LandfillCategory.Illegal
        };
    }
}