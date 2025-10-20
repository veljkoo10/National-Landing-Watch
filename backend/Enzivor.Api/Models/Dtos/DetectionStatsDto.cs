namespace Enzivor.Api.Models.Dtos
{
    public sealed class DetectionStatsDto
    {
        public int Total { get; set; }
        public int Linked { get; set; }
        public int Unlinked { get; set; }
        public int ReadyForPromotion { get; set; }
        public double AvgConfidence { get; set; }
    }
}