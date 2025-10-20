namespace Enzivor.Api.Models.Dtos
{
    public sealed class ProductionProcessResultDto
    {
        public int Processed { get; set; }
        public int Persisted { get; set; }
        public int WithSegmentation { get; set; }
        public int WithMetadata { get; set; }
    }
}