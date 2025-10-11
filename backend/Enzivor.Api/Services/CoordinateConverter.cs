using Enzivor.Api.Dtos;
using System.Globalization;

namespace Enzivor.Api.Services
{
    public class CoordinateConverter
    {
        // pretvara piksel koordinate u geo-koordinate koristeci poznate NW i SE tacke slike i dimenzije slike.
        public void ConvertPixelPolygonToGeoCoordinates(
           LandfillDto dto,
           double northWestLat,
           double northWestLon,
           double southEastLat,
           double southEastLon,
           int imageWidthPx,
           int imageHeightPx)
        {
            if (string.IsNullOrWhiteSpace(dto.PolygonCoordinates))
                return;

            // parsiranje stringa poligona: "x1,y1; x2,y2; ..."
            var points = dto.PolygonCoordinates
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(p =>
                {
                    var xy = p.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (xy.Length != 2) return (0.0, 0.0);

                    var x = double.Parse(xy[0], CultureInfo.InvariantCulture);
                    var y = double.Parse(xy[1], CultureInfo.InvariantCulture);
                    return (x, y);
                })
                .ToList();

            if (points.Count == 0) return;

            // linearna konverzija piksela u geografske koordinate
            // pretpostavka: (0,0) je gornji lijevi ugao slike

            var latMin = double.MaxValue;
            var latMax = double.MinValue;
            var lonMin = double.MaxValue;
            var lonMax = double.MinValue;

            foreach (var (px, py) in points)
            {
                // ratio piksela u odnosu na sirinu/visinu slike
                var xRatio = px / imageWidthPx;
                var yRatio = py / imageHeightPx;

                // Latitude: veca vrijednost je sjevernije, ali y raste ka dole
                var lat = northWestLat - yRatio * (northWestLat - southEastLat);
                // Longitude: lijevo (NW) ka desno (SE)
                var lon = northWestLon + xRatio * (southEastLon - northWestLon);

                latMin = Math.Min(latMin, lat);
                latMax = Math.Max(latMax, lat);
                lonMin = Math.Min(lonMin, lon);
                lonMax = Math.Max(lonMax, lon);
            }

            // Popuni DTO novim vrijednostima
            dto.NorthWestLat = latMax; // najveca latituda je na sjeveru
            dto.NorthWestLon = lonMin; // najmanja longituda je na zapadu
            dto.SouthEastLat = latMin; // najmanja latituda je na jugu
            dto.SouthEastLon = lonMax; // najveca longituda je na istoku
        }
    }
}

