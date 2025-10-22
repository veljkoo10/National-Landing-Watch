using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using Enzivor.Api.Models.Dtos.Landfills;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    public class ProductionLandfillProcessor : IProductionLandfillProcessor
    {
        private readonly ICalculationService _calculationService;

        public ProductionLandfillProcessor(ICalculationService calculationService)
        {
            _calculationService = calculationService;
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

            Console.WriteLine($"Processing {landfillClassifications.Count} landfill classifications out of {classifications.Count} total");
            Console.WriteLine($"L0 (methane potential): {_calculationService.GetMethaneGenerationPotential():F4} m³ CH4/tonne waste");

            foreach (var classification in landfillClassifications)
            {
                var segmentation = segmentations.FirstOrDefault(s => s.ImageName == classification.ImageName);

                var meta = metadata.FirstOrDefault(m => m.ImageName == classification.ImageName);

                if (meta == null)
                {
                    Console.WriteLine($"Skipping {classification.ImageName}: No metadata found");
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

                ProcessCoordinatesAndArea(dto);
                _calculationService.CalculateMethaneEmissions(dto);
                results.Add(dto);

                Console.WriteLine($"Processed {classification.ImageName}: {classification.Type} " +
                                  $"(confidence: {classification.Confidence:F2}, " +
                                  $"segmentation: {(segmentation != null ? "Yes" : "Default")}, " +
                                  $"Area: {dto.SurfaceArea:F2} m², CH4: {dto.CH4GeneratedTonnes:F2} t/year)");
            }

            return results;
        }

        // ===================== CSV READERS =====================

        private static string DetectDelimiter(string path)
        {
            var firstLine = File.ReadLines(path, Encoding.GetEncoding(1250)).FirstOrDefault() ?? "";
            return firstLine.Contains('\t') ? "\t" : ",";
        }

        private static StreamReader OpenWithEncoding(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var text = Encoding.UTF8.GetString(bytes);
            return text.Contains('�')
                ? new StreamReader(path, Encoding.GetEncoding(1250))
                : new StreamReader(path, Encoding.UTF8);
        }

        private async Task<List<ClassificationData>> ReadClassificationCsv(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = DetectDelimiter(path),
                IgnoreBlankLines = true,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                PrepareHeaderForMatch = args => args.Header?.Trim()
            };

            using var reader = OpenWithEncoding(path);
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<ClassificationDataMap>();
            var records = csv.GetRecords<ClassificationData>().ToList();

            await Task.CompletedTask;
            return records;
        }

        private async Task<List<SegmentationData>> ReadSegmentationCsv(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = DetectDelimiter(path),
                IgnoreBlankLines = true,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                PrepareHeaderForMatch = args => args.Header?.Trim()
            };

            using var reader = OpenWithEncoding(path);
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<SegmentationDataMap>();
            var records = csv.GetRecords<SegmentationData>().ToList();

            foreach (var r in records)
            {
                if (!string.IsNullOrWhiteSpace(r.PolygonCoordinates))
                {
                    r.PolygonCoordinates = r.PolygonCoordinates
                        .Replace("\"", "")
                        .Replace(" ", "")
                        .Trim();
                }
            }

            await Task.CompletedTask;
            return records;
        }

        private async Task<List<MetadataData>> ReadMetadataCsv(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = DetectDelimiter(path),
                IgnoreBlankLines = true,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                PrepareHeaderForMatch = args => args.Header?.Trim()
            };

            using var reader = OpenWithEncoding(path);
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<MetadataDataMap>();
            var records = csv.GetRecords<MetadataData>().ToList();

            foreach (var r in records)
            {
                if (!string.IsNullOrWhiteSpace(r.LandfillName))
                {
                    r.LandfillName = r.LandfillName.Trim().Trim('"', '“', '”').Replace("\"", string.Empty);
                }

                if (!string.IsNullOrWhiteSpace(r.NorthWestLatRaw))
                {
                    r.NorthWestLat = ParseCoordinate(r.NorthWestLatRaw);
                    r.NorthWestLon = ParseCoordinate(r.NorthWestLonRaw);
                    r.SouthEastLat = ParseCoordinate(r.SouthEastLatRaw);
                    r.SouthEastLon = ParseCoordinate(r.SouthEastLonRaw);
                }

                r.ZoomLevel = ParseZoomLevel(r.ZoomLevelRaw);
            }

            await Task.CompletedTask;
            return records;
        }

        // ===================== GEOMETRY PROCESSING =====================

        private void ProcessCoordinatesAndArea(LandfillDto dto)
        {
            try
            {
                var cleanPolygon = dto.PolygonCoordinates?
                    .Replace("\"", "")
                    .Replace("'", "")
                    .Trim() ?? CreateDefaultPolygon();

                var parts = cleanPolygon
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim().Split(','))
                    .Where(xy => xy.Length == 2)
                    .Where(xy => double.TryParse(xy[0], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                                 double.TryParse(xy[1], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    .Select(xy => new
                    {
                        X = double.Parse(xy[0], CultureInfo.InvariantCulture),
                        Y = double.Parse(xy[1], CultureInfo.InvariantCulture)
                    })
                    .ToList();

                if (parts.Count == 0)
                {
                    Console.WriteLine($"No valid points for {dto.ImageName}, using default polygon");
                    parts = CreateDefaultPolygon()
                        .Split(';')
                        .Select(p => p.Trim().Split(','))
                        .Select(xy => new { X = double.Parse(xy[0]), Y = double.Parse(xy[1]) })
                        .ToList();
                }

                var minLat = double.MaxValue; var maxLat = double.MinValue;
                var minLon = double.MaxValue; var maxLon = double.MinValue;

                foreach (var p in parts)
                {
                    var xRatio = p.X / 640.0;
                    var yRatio = p.Y / 640.0;

                    var lat = dto.ImageNorthWestLat - yRatio * (dto.ImageNorthWestLat - dto.ImageSouthEastLat);
                    var lon = dto.ImageNorthWestLon + xRatio * (dto.ImageSouthEastLon - dto.ImageNorthWestLon);

                    minLat = Math.Min(minLat, lat);
                    maxLat = Math.Max(maxLat, lat);
                    minLon = Math.Min(minLon, lon);
                    maxLon = Math.Max(maxLon, lon);
                }

                dto.NorthWestLat = maxLat;
                dto.NorthWestLon = minLon;
                dto.SouthEastLat = minLat;
                dto.SouthEastLon = maxLon;
                dto.CenterLat = (maxLat + minLat) / 2.0;
                dto.CenterLon = (maxLon + minLon) / 2.0;

                var baseLatDist = (maxLat - minLat) * 111_320;
                var baseLonDist = (maxLon - minLon) * 111_320 * Math.Cos(dto.CenterLat * Math.PI / 180);
                var baseArea = Math.Abs(baseLatDist * baseLonDist);

                var zoomFactor = dto.ZoomLevel.HasValue ? CalculateZoomPrecisionFactor(dto.ZoomLevel.Value) : 1.0;
                dto.SurfaceArea = baseArea * zoomFactor;

                Console.WriteLine($"Area {dto.ImageName}: Base={baseArea:F1} m², Zoom={dto.ZoomLevel}, Final={dto.SurfaceArea:F1} m²");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating area for {dto.ImageName}: {ex.Message}");
                dto.SurfaceArea = 0;
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

        // ===================== HELPERS =====================

        private string CreateDefaultPolygon() => "100,100;540,100;540,540;100,540";

        private static int? ParseZoomLevel(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var digits = Regex.Replace(raw, @"\D+", "");
            return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var z) ? z : null;
        }

        private double ParseCoordinate(string coord)
        {
            if (string.IsNullOrWhiteSpace(coord)) return 0;

            var clean = coord
                .Replace(',', '.')
                .Replace("°", " ")
                .Replace("º", " ")
                .Replace("’", "'")
                .Replace("‘", "'")
                .Replace("″", "\"")
                .Replace("”", "\"")
                .Replace("“", "\"")
                .Trim();

            var cardinal = clean.EndsWith("N", StringComparison.OrdinalIgnoreCase) ? 'N' :
                           clean.EndsWith("S", StringComparison.OrdinalIgnoreCase) ? 'S' :
                           clean.EndsWith("E", StringComparison.OrdinalIgnoreCase) ? 'E' :
                           clean.EndsWith("W", StringComparison.OrdinalIgnoreCase) ? 'W' : '\0';

            clean = clean.TrimEnd('N', 'S', 'E', 'W', ' ');

            if (double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
            {
                if (cardinal == 'S' || cardinal == 'W') dec = -dec;
                return dec;
            }

            var parts = Regex.Split(clean, @"\s+|\'|\""")
                             .Where(p => !string.IsNullOrWhiteSpace(p))
                             .ToArray();

            if (parts.Length >= 2 &&
                double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var deg) &&
                double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var min))
            {
                double sec = 0;
                if (parts.Length >= 3)
                    double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out sec);

                dec = deg + (min / 60.0) + (sec / 3600.0);
                if (cardinal == 'S' || cardinal == 'W') dec = -dec;
                return dec;
            }

            return 0;
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
                "kosovo i metohija" => SerbianRegion.KosovoMetohija,
                _ => null
            };
        }

        // ===================== INTERNAL CLASSES & MAPS =====================

        private class ClassificationData
        {
            public string ImageName { get; set; } = "";
            public string Type { get; set; } = "";
            public double Confidence { get; set; }
        }

        private class SegmentationData
        {
            public string ImageName { get; set; } = "";
            public double Confidence { get; set; }
            public string PolygonCoordinates { get; set; } = "";
        }

        private class MetadataData
        {
            public int Id { get; set; }
            public string ImageName { get; set; } = "";
            public string LandfillName { get; set; } = "";
            public string ImagePath { get; set; } = "";

            public string NorthWestLatRaw { get; set; } = "";
            public string NorthWestLonRaw { get; set; } = "";
            public string SouthEastLatRaw { get; set; } = "";
            public string SouthEastLonRaw { get; set; } = "";

            public string RegionTag { get; set; } = "";
            public string? ZoomLevelRaw { get; set; }
            public int? ZoomLevel { get; set; }

            public double NorthWestLat { get; set; }
            public double NorthWestLon { get; set; }
            public double SouthEastLat { get; set; }
            public double SouthEastLon { get; set; }
        }

        private sealed class ClassificationDataMap : ClassMap<ClassificationData>
        {
            public ClassificationDataMap()
            {
                Map(m => m.ImageName).Name("image_name", "ImageName");
                Map(m => m.Type).Name("predicted_label", "Type");
                Map(m => m.Confidence).Name("confidence", "Confidence");
            }
        }

        private sealed class SegmentationDataMap : ClassMap<SegmentationData>
        {
            public SegmentationDataMap()
            {
                Map(m => m.ImageName).Name("image_name", "ImageName");
                Map(m => m.Confidence).Optional();
                Map(m => m.PolygonCoordinates).Name("polygon_px", "PolygonCoordinates");
            }
        }

        private sealed class MetadataDataMap : ClassMap<MetadataData>
        {
            public MetadataDataMap()
            {
                Map(m => m.Id).Name("Id", "id");
                Map(m => m.ImageName).Name("ImageName", "image_name");
                Map(m => m.LandfillName).Name("LandfillName", "landfill_name");
                Map(m => m.ImagePath).Name("ImagePath", "image_path");

                Map(m => m.NorthWestLatRaw).Name("NorthWestLat", "north_west_lat");
                Map(m => m.NorthWestLonRaw).Name("NorthWestLon", "north_west_lon");
                Map(m => m.SouthEastLatRaw).Name("SouthEastLat", "south_east_lat");
                Map(m => m.SouthEastLonRaw).Name("SouthEastLon", "south_east_lon");

                Map(m => m.RegionTag).Name("RegionTag", "region_tag");
                Map(m => m.ZoomLevelRaw).Name("ZoomLevel", "zoom_level").Optional();
            }
        }
    }
}