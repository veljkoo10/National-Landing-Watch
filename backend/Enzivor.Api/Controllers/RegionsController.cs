using Enzivor.Api.Models.Enums;
using Enzivor.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Enzivor.Api.Controllers
{
    [ApiController]
    [Route("api/regions")]
    public class RegionsController : ControllerBase
    {
        private readonly ILandfillSiteRepository _siteRepository;

        public RegionsController(ILandfillSiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRegions()
        {
            var staticRegions = new Dictionary<string, (int Id, string Name, int Population, int AreaKm2)>
            {
                { "Vojvodina", (1, "Vojvodina", 1900000, 21506) },          
                { "Beograd", (2, "Beograd", 1675000, 3226) },               
                { "Zapadna Srbija", (3, "Zapadna Srbija", 1200000, 26000) }, 
                { "Šumadija i Pomoravlje", (4, "Šumadija i Pomoravlje", 1000000, 13000) }, 
                { "Istočna Srbija", (5, "Istočna Srbija", 800000, 19000) },  
                { "Južna Srbija", (6, "Južna Srbija", 900000, 15000) }     
            };

            var allSites = await _siteRepository.GetAllAsync(default);

            var regions = staticRegions.Select(kvp =>
            {
                var regionSites = allSites.Where(s => s.RegionTag == kvp.Key).ToList();
                var landfillCount = regionSites.Count;
                var ch4Tons = regionSites.Sum(s => s.EstimatedCH4TonsPerYear ?? 0);
                var co2Tons = regionSites.Sum(s => s.EstimatedCO2eTonsPerYear ?? 0);
                var totalWaste = regionSites.Sum(s => s.AnnualMSWTons ?? 0);

                return new
                {
                    kvp.Value.Id,
                    kvp.Value.Name,
                    kvp.Value.Population,
                    AreaKm2 = Math.Round((double)kvp.Value.AreaKm2, 2),
                    LandfillCount = landfillCount,
                    Ch4Tons = Math.Round(ch4Tons, 2),
                    Co2Tons = Math.Round(co2Tons, 2),
                    TotalWaste = Math.Round(totalWaste, 2),
                    PollutionLevel = GetPollutionLevel(ch4Tons, kvp.Value.AreaKm2)
                };
            }).ToList();

            return Ok(regions);
        }

        private string GetPollutionLevel(double ch4TonsPerYear, int areaKm2)
        {
            var ch4Density = ch4TonsPerYear / areaKm2;

            return ch4Density switch
            {
                > 2.0 => "high",     
                > 0.5 => "medium",   
                _ => "low"         
            };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRegionById(int id)
        {
            var regionNames = new Dictionary<int, string>
            {
                { 1, "Vojvodina" }, { 2, "Belgrade" }, { 3, "Western Serbia" },
                { 4, "Sumadija And Pomoravlje" }, { 5, "Eastern Serbia" }, { 6, "Southern Serbia" }
            };

            if (!regionNames.TryGetValue(id, out var regionName))
                return NotFound();

            var sites = await _siteRepository.GetByRegionAsync(regionName, default);

            var result = new
            {
                Id = id,
                Name = regionName,
                LandfillCount = sites.Count,
                Population = id switch { 1 => 1900000, 2 => 1675000, 3 => 1200000, 4 => 1000000, 5 => 800000, 6 => 900000, _ => 0 },
                AreaKm2 = id switch { 1 => 21506, 2 => 3226, 3 => 26000, 4 => 13000, 5 => 19000, 6 => 15000, _ => 0 },
                Ch4Tons = Math.Round(sites.Sum(s => s.EstimatedCH4TonsPerYear ?? 0), 2),
                Co2Tons = Math.Round(sites.Sum(s => s.EstimatedCO2eTonsPerYear ?? 0), 2),
                PollutionLevel = GetPollutionLevel(sites.Sum(s => s.EstimatedCH4TonsPerYear ?? 0), id switch { 1 => 21506, 2 => 3226, 3 => 26000, 4 => 13000, 5 => 19000, 6 => 15000, _ => 1 }),
                TotalWaste = Math.Round(sites.Sum(s => s.AnnualMSWTons ?? 0), 2)
            };

            return Ok(result);
        }

        [HttpGet("key/{key}")]
        public async Task<IActionResult> GetRegionByKey(string key)
        {
            var regionMap = new Dictionary<string, (int Id, string Name)>
            {
                { "vojvodina", (1, "Vojvodina") },
                { "beograd", (2, "Beograd") },                       
                { "zapadnasrbija", (3, "Zapadna Srbija") },         
                { "sumadijaipomoravlje", (4, "Šumadija i Pomoravlje") }, 
                { "istocnasrbija", (5, "Istočna Srbija") },        
                { "juznasrbija", (6, "Južna Srbija") }              
            };

            if (!regionMap.TryGetValue(key.ToLowerInvariant(), out var region))
                return NotFound();

            var sites = await _siteRepository.GetByRegionAsync(region.Name, default);

            var result = new
            {
                region.Id,
                Key = key,
                Name = region.Name,
                LandfillCount = sites.Count,
                Population = region.Id switch { 1 => 1900000, 2 => 1675000, 3 => 1200000, 4 => 1000000, 5 => 800000, 6 => 900000, _ => 0 },
                AreaKm2 = region.Id switch { 1 => 21506, 2 => 3226, 3 => 26000, 4 => 13000, 5 => 19000, 6 => 15000, _ => 0 },
                Ch4Tons = Math.Round(sites.Sum(s => s.EstimatedCH4TonsPerYear ?? 0), 2),
                Co2Tons = Math.Round(sites.Sum(s => s.EstimatedCO2eTonsPerYear ?? 0), 2),
                PollutionLevel = GetPollutionLevel(sites.Sum(s => s.EstimatedCH4TonsPerYear ?? 0), region.Id switch { 1 => 21506, 2 => 3226, 3 => 26000, 4 => 13000, 5 => 19000, 6 => 15000, _ => 1 }),
                TotalWaste = Math.Round(sites.Sum(s => s.AnnualMSWTons ?? 0), 2)
            };

            return Ok(result);
        }
    }
}