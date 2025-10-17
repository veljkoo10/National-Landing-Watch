using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    public class MethaneCalculationService : IMethaneCalculationService
    {
        private const double L0 = 0.1;              // Methane potential (simplified)
        private const double K_DEFAULT = 0.06;      // Decay rate
        private const double CH4_TO_CO2 = 25.0;     // CO2 equivalent factor

        public void CalculateMethaneEmissions(LandfillDto dto)
        { 
            if (dto?.SurfaceArea <= 0) return;

            var wasteDepth = GetSimpleDepth(dto.SurfaceArea);
            var wasteDensity = 0.7; 
            var totalWaste = dto.SurfaceArea * wasteDepth * wasteDensity;

            var decayRate = GetRegionDecayRate(dto.ParsedRegion);

            var methanePerYear = totalWaste * L0 * decayRate;

            dto.EstimatedDepth = wasteDepth;
            dto.EstimatedDensity = wasteDensity;
            dto.EstimatedMSW = totalWaste;
            dto.MCF = wasteDepth >= 3.0 ? 0.8 : 0.4;
            dto.CH4GeneratedTonnesPerYear = methanePerYear;
            dto.CO2EquivalentTonnesPerYear = methanePerYear * CH4_TO_CO2;
        }

        public void CalculateMethaneEmissions(IEnumerable<LandfillDto> landfills)
        {
            foreach (var landfill in landfills)
            {
                CalculateMethaneEmissions(landfill);
            }
        }

        public double GetMethaneGenerationPotential()
        {
            return L0;
        }

        public double GetDecayRateForRegion(SerbianRegion? region)
        {
            return GetRegionDecayRate(region);
        }

        private double GetSimpleDepth(double areaM2)
        {
            if (areaM2 <= 5000) return 1.5;      // Small dumps
            if (areaM2 <= 20000) return 3.0;     // Medium dumps  
            if (areaM2 <= 100000) return 5.0;    // Large dumps
            return 7.0;                          // Very large dumps
        }

        private double GetRegionDecayRate(SerbianRegion? region)
        {
            return region switch
            {
                SerbianRegion.Vojvodina => 0.065,        // Warmer region
                SerbianRegion.Belgrade => 0.06,          // Urban area
                SerbianRegion.WesternSerbia => 0.055,    // Cooler mountains
                SerbianRegion.EasternSerbia => 0.055,    // Cooler mountains
                SerbianRegion.SouthernSerbia => 0.058,   // Moderate
                SerbianRegion.SumadijaPomoravlje => 0.06, // Moderate
                _ => K_DEFAULT                           // Default
            };
        }
    }
}
