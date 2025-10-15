using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Services.Interfaces;
using System.Globalization;

namespace Enzivor.Api.Services.Implementations
{
    public class CoordinateConverter : ICoordinateConverter
    {
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

            var latMin = double.MaxValue;
            var latMax = double.MinValue;
            var lonMin = double.MaxValue;
            var lonMax = double.MinValue;

            foreach (var (px, py) in points)
            {
                var xRatio = px / imageWidthPx;
                var yRatio = py / imageHeightPx;

                var lat = northWestLat - yRatio * (northWestLat - southEastLat);
                var lon = northWestLon + xRatio * (southEastLon - northWestLon);

                latMin = Math.Min(latMin, lat);
                latMax = Math.Max(latMax, lat);
                lonMin = Math.Min(lonMin, lon);
                lonMax = Math.Max(lonMax, lon);
            }

            dto.NorthWestLat = latMax;
            dto.NorthWestLon = lonMin;
            dto.SouthEastLat = latMin; 
            dto.SouthEastLon = lonMax;
        }
    }
}


