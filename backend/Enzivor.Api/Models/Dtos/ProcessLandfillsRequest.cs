namespace Enzivor.Api.Models.Dtos
{
    public class ProcessLandfillsRequest
    {
        public string CsvPath { get; set; } = string.Empty;
        public double NorthWestLat { get; set; }
        public double NorthWestLon { get; set; }
        public double SouthEastLat { get; set; }
        public double SouthEastLon { get; set; }
        public int ImageWidthPx { get; set; }
        public int ImageHeightPx { get; set; }
    }
}
