namespace Enzivor.Api.Models.Dtos.Users
{
    public class UserReportDto
    {
        public int Id { get; set; }
        public string? ReporterEmail { get; set; }
        public string? PlaceName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Description { get; set; }
        public bool IsReviewed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}