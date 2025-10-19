using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/landfills")]
    public class LandfillsController : ControllerBase
    {
        private readonly ILandfillSiteRepository _siteRepository;
        private readonly ILandfillSiteService _siteService;

        public LandfillsController(ILandfillSiteRepository siteRepository, ILandfillSiteService siteService)
        {
            _siteRepository = siteRepository;
            _siteService = siteService;
        }

        // GET: api/landfills
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShowLandfillDto>>> GetAllLandfills(CancellationToken ct)
        {
            var sites = await _siteRepository.GetAllAsync(ct);

            var landfills = sites.Select(s => ToShowLandfillDto(
                id: s.Id,
                name: s.Name,
                regionKey: (s.RegionTag ?? "unknown"),
                latitude: s.PointLat ?? 0,
                longitude: s.PointLon ?? 0,
                backendCategory: s.Category.ToString(),
                areaM2: s.EstimatedAreaM2,
                yearCreated: s.StartYear
            )).ToList();

            return Ok(landfills);
        }

        // GET: api/landfills/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ShowLandfillDto>> GetLandfillById(int id, CancellationToken ct)
        {
            var s = await _siteRepository.GetByIdAsync(id, ct);
            if (s is null) return NotFound();

            var dto = ToShowLandfillDto(
                id: s.Id,
                name: s.Name,
                regionKey: (s.RegionTag ?? "unknown"),
                latitude: s.PointLat ?? 0,
                longitude: s.PointLon ?? 0,
                backendCategory: s.Category.ToString(),
                areaM2: s.EstimatedAreaM2,
                yearCreated: s.StartYear
            );

            return Ok(dto);
        }

        // GET: api/landfills/region/{regionKey}
        [HttpGet("region/{regionKey}")]
        public async Task<ActionResult<IEnumerable<ShowLandfillDto>>> GetLandfillsByRegion(string regionKey, CancellationToken ct)
        {
            var key = Normalize(regionKey);
            if (string.IsNullOrWhiteSpace(key))
                return Ok(new List<ShowLandfillDto>());

            // map canonical key -> DB display name
            var displayName = key switch
            {
                "vojvodina" => "Vojvodina",
                "beograd" => "Beograd",
                "zapadnasrbija" => "Zapadna Srbija",
                "sumadijaipomoravlje" => "Šumadija i Pomoravlje",
                "istocnasrbija" => "Istočna Srbija",
                "juznasrbija" => "Južna Srbija",
                _ => key
            };

            var sites = await _siteRepository.GetByRegionAsync(displayName, ct); // <-- query by display name

            var landfills = sites.Select(s => ToShowLandfillDto(
                id: s.Id,
                name: s.Name,
                regionKey: key,                   // keep canonical key for FE
                latitude: s.PointLat ?? 0,
                longitude: s.PointLon ?? 0,
                backendCategory: s.Category.ToString(),
                areaM2: s.EstimatedAreaM2,
                yearCreated: s.StartYear
            )).ToList();

            return Ok(landfills);
        }

        [HttpPost("promote")]
        public async Task<IActionResult> PromoteDetectionsToSites(
            [FromQuery] double? minConfidence = null,
            [FromQuery] LandfillCategory? category = null,
            CancellationToken ct = default)
        {
            var createdCount = await _siteService.CreateSitesFromUnlinkedDetectionsAsync(
                minConfidence, category, ct
            );

            if (createdCount == 0)
                return Ok(new { created = 0, message = "No new sites were created. All detections are already linked." });

            return Ok(new { created = createdCount, message = $"{createdCount} new landfill sites were created." });
        }

        // -------------------------
        // Mapping & helpers
        // -------------------------

        private static ShowLandfillDto ToShowLandfillDto(
            int id,
            string? name,
            string regionKey,
            double latitude,
            double longitude,
            string? backendCategory,
            double? areaM2,
            int? yearCreated)
        {
            var type = GetFrontendType(backendCategory);
            var size = GetSizeFromArea(areaM2);

            return new ShowLandfillDto
            {
                Id = id,
                Name = name,
                RegionKey = regionKey,
                Latitude = latitude,
                Longitude = longitude,
                Type = type,
                Size = size,
                YearCreated = yearCreated ?? 2020,
                AreaM2 = areaM2 is null ? null : Math.Round(areaM2.Value, 2)
            };
        }

        private static string GetFrontendType(string? category)
        {
            switch ((category ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "illegal": return "wild";
                case "nonsanitary": return "unsanitary";
                case "sanitary": return "sanitary";
                default: return "unsanitary";
            }
        }

        private static string GetSizeFromArea(double? areaM2)
        {
            if (!areaM2.HasValue) return "small";
            var a = areaM2.Value;
            if (a <= 10_000) return "small";
            if (a <= 50_000) return "medium";
            return "large";
        }

        private static string Normalize(string? s) =>
     string.IsNullOrWhiteSpace(s)
         ? ""
         : new string(s.Trim().ToLowerInvariant().Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
}
