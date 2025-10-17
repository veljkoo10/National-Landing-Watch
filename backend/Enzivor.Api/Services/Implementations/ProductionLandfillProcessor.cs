using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    public class ProductionLandfillProcessor : IProductionLandfillProcessor
    {

        private readonly IMethaneCalculationService _methaneService;

        public ProductionLandfillProcessor(IMethaneCalculationService methaneService)
        {
            _methaneService = methaneService;
        }

        public async Task<List<LandfillDto>> ProcessProductionData(
       string classificationCsvPath,
       string segmentationCsvPath,
       string metadataSpreadsheetPath)
        {
            var classifications = await ReadClassificationCsv(classificationCsvPath);
            var segmentations = await ReadSegmentationCsv(segmentationCsvPath);
            var metadata = await ReadMetadataCsv(metadataSpreadsheetPath);

            var results = new List<LandfillDto>();

            var landfillClassifications = classifications
                .Where(c => c.Type == "illegal" || c.Type == "non_illegal")
                .ToList();

            Console.WriteLine($"🎯 Processing {landfillClassifications.Count} landfill classifications out of {classifications.Count} total");
            Console.WriteLine($"📐 L0 (methane potential): {_methaneService.GetMethaneGenerationPotential():F4} m³ CH4/tonne waste");

            foreach (var classification in landfillClassifications)
            {
                var segmentation = segmentations.FirstOrDefault(s => s.ImageName == classification.ImageName);
                var meta = metadata.FirstOrDefault(m => m.ImageName == classification.ImageName);

                if (meta == null)
                {
                    Console.WriteLine($"⚠️ Skipping {classification.ImageName}: No metadata found");
                    continue;
                }

                var dto = new LandfillDto
                {
                    ImageName = classification.ImageName,
                    Type = classification.Type,
                    Confidence = classification.Confidence,
                    PolygonCoordinates = segmentation?.PolygonCoordinates ?? CreateDefaultPolygon(),

                    KnownLandfillName = meta.LandfillName,
                    RegionTag = meta.RegionTag,
                    ParsedRegion = ParseRegion(meta.RegionTag),
                    ZoomLevel = meta.ZoomLevel,

                    ImageNorthWestLat = meta.NorthWestLat,
                    ImageNorthWestLon = meta.NorthWestLon,
                    ImageSouthEastLat = meta.SouthEastLat,
                    ImageSouthEastLon = meta.SouthEastLon
                };

                // 4. Calculate coordinates and area
                ProcessCoordinatesAndArea(dto);

                // 5. Calculate methane emissions using dedicated service
                _methaneService.CalculateMethaneEmissions(dto);

                results.Add(dto);

                Console.WriteLine($"✅ Processed {classification.ImageName}: {classification.Type} " +
                                 $"(confidence: {classification.Confidence:F2}, " +
                                 $"segmentation: {(segmentation != null ? "Yes" : "Default")}, " +
                                 $"CH4: {dto.CH4GeneratedTonnesPerYear:F2} t/year)");
            }

            return results;
        }

        private async Task<List<ClassificationData>> ReadClassificationCsv(string path)
        {
            var results = new List<ClassificationData>();
            var lines = await File.ReadAllLinesAsync(path);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length >= 3)
                {
                    results.Add(new ClassificationData
                    {
                        ImageName = parts[0].Trim(),
                        Type = parts[1].Trim(),
                        Confidence = double.Parse(parts[2].Trim())
                    });
                }
            }
            return results;
        }

        private async Task<List<SegmentationData>> ReadSegmentationCsv(string path)
        {
            var results = new List<SegmentationData>();
            var lines = await File.ReadAllLinesAsync(path);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length >= 3)
                {
                    var polygon = parts[2]
                .Replace("\"", "")        
                .Replace(";", "; ")       
                .Trim();
                    results.Add(new SegmentationData
                    {
                        ImageName = parts[0].Trim(),
                        PolygonCoordinates = polygon
                    });
                }
            }
            return results;
        }

        private async Task<List<MetadataData>> ReadMetadataCsv(string path)
        {
            var results = new List<MetadataData>();
            var lines = await File.ReadAllLinesAsync(path);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length >= 10)
                {
                    results.Add(new MetadataData
                    {
                        ImageName = parts[1].Trim(),
                        LandfillName = parts[2].Trim(),
                        NorthWestLat = ParseCoordinate(parts[4]),
                        NorthWestLon = ParseCoordinate(parts[5]),
                        SouthEastLat = ParseCoordinate(parts[6]),
                        SouthEastLon = ParseCoordinate(parts[7]),
                        RegionTag = parts[8].Trim(),
                        ZoomLevel = int.TryParse(parts[9].Trim(), out var zoom) ? zoom : null
                    });
                }
            }
            return results;
        }

        private void ProcessCoordinatesAndArea(LandfillDto dto)
        {
            try
            {
                // ✅ Clean the polygon string thoroughly
                var cleanPolygon = dto.PolygonCoordinates?
                    .Replace("\"", "")           // Remove quotes
                    .Replace("'", "")            // Remove single quotes
                    .Replace("polygon_px", "")   // Remove column header if present
                    .Trim() ?? CreateDefaultPolygon();

                Console.WriteLine($"🔍 Debug: Processing polygon for {dto.ImageName}: '{cleanPolygon}'");

                var points = cleanPolygon
                    .Split(';')
                    .Where(p => !string.IsNullOrWhiteSpace(p))      // ✅ Filter empty parts
                    .Select(p => p.Trim().Split(','))
                    .Where(xy => xy.Length >= 2)                    // ✅ Ensure we have X,Y pairs
                    .Where(xy => double.TryParse(xy[0].Trim(), out _) &&
                                double.TryParse(xy[1].Trim(), out _)) // ✅ Ensure both are valid numbers
                    .Select(xy => new {
                        X = double.Parse(xy[0].Trim()),
                        Y = double.Parse(xy[1].Trim())
                    })
                    .ToList();

                // ✅ Fallback to default polygon if no valid points
                if (points.Count == 0)
                {
                    Console.WriteLine($"⚠️ No valid points found for {dto.ImageName}, using default polygon");
                    var defaultPolygon = CreateDefaultPolygon();
                    points = defaultPolygon
                        .Split(';')
                        .Select(p => p.Trim().Split(','))
                        .Select(xy => new { X = double.Parse(xy[0]), Y = double.Parse(xy[1]) })
                        .ToList();
                }

                Console.WriteLine($"✅ Found {points.Count} valid points for {dto.ImageName}");

                var minLat = double.MaxValue; var maxLat = double.MinValue;
                var minLon = double.MaxValue; var maxLon = double.MinValue;

                foreach (var point in points)
                {
                    // ✅ FIXED: Use 640 instead of 1024 for YOLO model
                    var xRatio = point.X / 640.0;
                    var yRatio = point.Y / 640.0;

                    var lat = dto.ImageNorthWestLat - yRatio * (dto.ImageNorthWestLat - dto.ImageSouthEastLat);
                    var lon = dto.ImageNorthWestLon + xRatio * (dto.ImageSouthEastLon - dto.ImageNorthWestLon);

                    minLat = Math.Min(minLat, lat); maxLat = Math.Max(maxLat, lat);
                    minLon = Math.Min(minLon, lon); maxLon = Math.Max(maxLon, lon);
                }

                dto.NorthWestLat = maxLat; dto.NorthWestLon = minLon;
                dto.SouthEastLat = minLat; dto.SouthEastLon = maxLon;
                dto.CenterLat = (maxLat + minLat) / 2.0;
                dto.CenterLon = (maxLon + minLon) / 2.0;

                // Calculate area with zoom precision
                var baseLatDist = (maxLat - minLat) * 111320;
                var baseLonDist = (maxLon - minLon) * 111320 * Math.Cos(dto.CenterLat * Math.PI / 180);
                var baseArea = Math.Abs(baseLatDist * baseLonDist);

                if (dto.ZoomLevel.HasValue)
                {
                    var zoomFactor = CalculateZoomPrecisionFactor(dto.ZoomLevel.Value);
                    dto.SurfaceArea = baseArea * zoomFactor;

                    Console.WriteLine($"📏 Area calculation for {dto.ImageName}: " +
                                     $"Base: {baseArea:F1}m², " +
                                     $"Zoom: {dto.ZoomLevel}, " +
                                     $"Factor: {zoomFactor:F3}, " +
                                     $"Final: {dto.SurfaceArea:F1}m²");
                }
                else
                {
                    dto.SurfaceArea = baseArea;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing coordinates for {dto.ImageName}: {ex.Message}");
                Console.WriteLine($"❌ Polygon data was: '{dto.PolygonCoordinates}'");

                // Use default polygon as fallback
                var defaultPolygon = CreateDefaultPolygon();
                dto.PolygonCoordinates = defaultPolygon;

                // Recursively call with default polygon
                ProcessCoordinatesAndArea(dto);
            }
        }

        private double CalculateZoomPrecisionFactor(int zoomLevel)
        {
            if (zoomLevel >= 1200) return 1.1;   
            if (zoomLevel >= 1000) return 1.05; 
            if (zoomLevel >= 800) return 1.0;    
            if (zoomLevel >= 600) return 0.95;  

            return 0.9; 
        }

        private string CreateDefaultPolygon() => "100,100; 540,100; 540,540; 100,540";

        private double ParseCoordinate(string coord)
        {
            var clean = coord.Replace("°", ".").Replace("'", ".").Replace("\"", "")
                           .Replace("N", "").Replace("S", "").Replace("E", "").Replace("W", "");
            return double.TryParse(clean, out var result) ? result : 0.0;
        }

        private SerbianRegion? ParseRegion(string? regionStr)
        {
            return regionStr?.ToLowerInvariant() switch
            {
                "vojvodina" => SerbianRegion.Vojvodina,
                "beograd" => SerbianRegion.Belgrade,
                "sumadija i pomoravlje" => SerbianRegion.SumadijaPomoravlje,
                "zapadna srbija" => SerbianRegion.WesternSerbia,
                "istocna srbija" => SerbianRegion.EasternSerbia,
                "juzna srbija" => SerbianRegion.SouthernSerbia,
                _ => null
            };
        }

        private class ClassificationData
        {
            public string ImageName { get; set; } = "";
            public string Type { get; set; } = "";
            public double Confidence { get; set; }
        }

        private class SegmentationData
        {
            public string ImageName { get; set; } = "";
            public string? PolygonCoordinates { get; set; }
        }

        private class MetadataData
        {
            public string ImageName { get; set; } = "";
            public string? LandfillName { get; set; }
            public double NorthWestLat { get; set; }
            public double NorthWestLon { get; set; }
            public double SouthEastLat { get; set; }
            public double SouthEastLon { get; set; }
            public string? RegionTag { get; set; }
            public int? ZoomLevel { get; set; }
        }
    }
}
