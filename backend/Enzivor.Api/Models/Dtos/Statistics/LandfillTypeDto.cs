namespace Enzivor.Api.Models.Dtos.Statistics
{
    public class LandfillTypeDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }

        public LandfillTypeDto() { }

        public LandfillTypeDto(string name, int count)
        {
            Name = name;
            Count = count;
        }
    }
}
