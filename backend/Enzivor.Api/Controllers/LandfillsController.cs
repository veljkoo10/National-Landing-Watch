using Enzivor.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    // poziv u swaggeru:
    // POST http://localhost:5000/api/landfills/process

    [ApiController]
    [Route("api/[controller]")]
    public class LandfillsController : ControllerBase
    {
        private readonly LandfillProcessingService _landfillService;

        public LandfillsController()
        {
            _landfillService = new LandfillProcessingService();
        }
        
        // obradjuje rezultate iz Python modela (CSV fajl) i vraca sve detektovane deponije
     
        [HttpPost("process")]
        public IActionResult ProcessLandfills(
            [FromQuery] string csvPath,
            [FromQuery] double northWestLat,
            [FromQuery] double northWestLon,
            [FromQuery] double southEastLat,
            [FromQuery] double southEastLon,
            [FromQuery] int imageWidthPx,
            [FromQuery] int imageHeightPx
        )
        {
            if (!System.IO.File.Exists(csvPath))
                return BadRequest($"CSV fajl nije pronađen na putanji: {csvPath}");

            // pokrece glavni servis za obradu CSV-a
            var results = _landfillService.ProcessLandfills(
                csvPath,
                northWestLat,
                northWestLon,
                southEastLat,
                southEastLon,
                imageWidthPx,
                imageHeightPx
            );

            // vraca rezultate frontendu (ili ih kasnije cuvamo u bazu?)
            return Ok(results);
        }
    }
}

// query parametri:

//csvPath = C:/ projekat / outputs / predictions.csv
//northWestLat = 44.12345
//northWestLon = 19.98765
//southEastLat = 43.98765
//southEastLon = 20.12345
//imageWidthPx = 1024
//imageHeightPx = 1024


// rezultat koji backend vraca:

//[
//  {
//    "imageName": "deponija_001.jpg",
//    "type": "illegal",
//    "confidence": 0.93,
//    "surfaceArea": 51234.56,
//    "northWestLat": 44.1231,
//    "northWestLon": 19.9880,
//    "southEastLat": 43.9881,
//    "southEastLon": 20.1223,
//    "centerLat": 44.0556,
//    "centerLon": 20.0551,
//    "polygonCoordinates": "123.5,245.7;150.1,270.4;..."
//  },
//  ...
//]


