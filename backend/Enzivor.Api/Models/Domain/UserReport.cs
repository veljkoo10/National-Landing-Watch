namespace Enzivor.Api.Models.Domain
{
    public class UserReport
    {
        public int Id { get; set; }
        public string? ReporterEmail { get; set; }
        public string? PlaceName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Description { get; set; }
        public bool IsReviewed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }
}