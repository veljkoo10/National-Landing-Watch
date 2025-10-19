namespace Enzivor.Api.Models.Dtos
{
    public class MonitoringDto
    {
        public int Id { get; set; }
        public int LandfillId { get; set; }
        public int Year { get; set; }
        public double AreaM2 { get; set; }
        public double VolumeM3 { get; set; }
        public double WasteTons { get; set; }
        public double Ch4Tons { get; set; }
        public double Co2Tons { get; set; }
    }
}
