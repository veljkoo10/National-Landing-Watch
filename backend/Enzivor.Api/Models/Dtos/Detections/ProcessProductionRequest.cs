namespace Enzivor.Api.Models.Dtos.Detections
{
    public class ProcessProductionRequest
    {
        public string ClassificationCsvPath { get; set; } = string.Empty;
        public string SegmentationCsvPath { get; set; } = string.Empty;
        public string MetadataSpreadsheetPath { get; set; } = string.Empty;
    }
}
