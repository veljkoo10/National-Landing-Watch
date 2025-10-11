namespace Enzivor.Api.Models
{
    public class Landfill
    {
        public int Id { get; set; }                     // jedinstveni ID u bazi
        public string ImageName { get; set; } = "";    // naziv slike iz koje potice detekcija
        public string Type { get; set; } = "";         // npr. illegal, non_illegal
        public double Confidence { get; set; }         // sigurnost modela 0.0 - 1.0
        public double SurfaceArea { get; set; }        // površina u m²

        // geografski podaci
        // sjeverozapadna (gore-lijevo) tacka 
        public double NorthWestLat { get; set; }
        public double NorthWestLon { get; set; }

        // jugoistocna (dole-desno) tacka
        public double SouthEastLat { get; set; }
        public double SouthEastLon { get; set; }

        // opciono ako cemo cuvati cijeli poligon kao tekst
        public string? PolygonCoordinates { get; set; }
    }
}
