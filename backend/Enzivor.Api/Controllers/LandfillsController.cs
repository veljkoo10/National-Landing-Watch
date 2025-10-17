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

        public LandfillsController(ILandfillSiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLandfills()
        {
            var sites = await _siteRepository.GetAllAsync(default);

            var landfills = sites.Select(s => new
            {
                Id = s.Id,
                Name = s.Name,
                RegionKey = s.RegionTag?.ToLowerInvariant() ?? "unknown",
                Latitude = s.PointLat ?? 0,
                Longitude = s.PointLon ?? 0,
                Type = GetFrontendType(s.Category.ToString()),
                Size = GetSizeFromArea(s.EstimatedAreaM2),
                YearCreated = s.StartYear ?? 2020,
                AreaM2 = Math.Round(s.EstimatedAreaM2 ?? 0, 2)
            }).ToList();

            return Ok(landfills);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLandfillById(int id)
        {
            var site = await _siteRepository.GetByIdAsync(id, default);
            if (site == null)
                return NotFound();

            var result = new
            {
                Id = site.Id,
                Name = site.Name,
                RegionKey = site.RegionTag?.ToLowerInvariant() ?? "unknown",
                Latitude = site.PointLat ?? 0,
                Longitude = site.PointLon ?? 0,
                Type = GetFrontendType(site.Category.ToString()),
                Size = GetSizeFromArea(site.EstimatedAreaM2),
                YearCreated = site.StartYear ?? 2020,
                AreaM2 = Math.Round(site.EstimatedAreaM2 ?? 0, 2)
            };

            return Ok(result);
        }

        [HttpGet("region/{regionKey}")]
        public async Task<IActionResult> GetLandfillsByRegion(string regionKey)
        {
            var sites = await _siteRepository.GetByRegionAsync(GetRegionNameFromKey(regionKey), default);

            var landfills = sites.Select(s => new
            {
                Id = s.Id,
                Name = s.Name,
                RegionKey = regionKey.ToLowerInvariant(),
                Latitude = s.PointLat ?? 0,
                Longitude = s.PointLon ?? 0,
                Type = GetFrontendType(s.Category.ToString()),
                Size = GetSizeFromArea(s.EstimatedAreaM2),
                YearCreated = s.StartYear ?? 2020,
                AreaM2 = Math.Round(s.EstimatedAreaM2 ?? 0, 2)
            }).ToList();

            return Ok(landfills);
        }

        private string GetFrontendType(string category)
        {
            return category.ToLowerInvariant() switch
            {
                "illegal" => "wild",
                "nonsanitary" => "unsanitary",
                "sanitary" => "sanitary",
                _ => "unsanitary"
            };
        }

        private string GetSizeFromArea(double? areaM2)
        {
            if (!areaM2.HasValue) return "small";

            return areaM2.Value switch
            {
                <= 10000 => "small",
                <= 50000 => "medium",
                _ => "large"
            };
        }

        private string GetRegionNameFromKey(string key)
        {
            return key.ToLowerInvariant() switch
            {
                "vojvodina" => "Vojvodina",
                "beograd" => "Beograd",                            
                "zapadnasrbija" => "Zapadna Srbija",                 
                "sumadijaipomoravlje" => "Šumadija i Pomoravlje",      
                "istocnasrbija" => "Istočna Srbija",                 
                "juznasrbija" => "Južna Srbija",                     
                _ => key
            };
        }
    }
}
