using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    public class CalculationService : ICalculationService
    {
        private const double L0 = 0.1;              // Methane potential (simplified)
        private const double K_DEFAULT = 0.06;      // Decay rate
        private const double CH4_TO_CO2 = 25.0;     // CO2 equivalent factor

        public void CalculateMethaneEmissions(LandfillDto dto)
        { 
            if (dto?.SurfaceArea <= 0) return;

            var wasteDepth = GetSimpleDepth(dto.SurfaceArea, dto.Type);
            var wasteDensity = GetSimpleDensity(dto.SurfaceArea, dto.Type);
            var totalWaste = dto.SurfaceArea * wasteDepth * wasteDensity;
            var decayRate = GetRegionDecayRate(dto.ParsedRegion);
            var methanePerYear = totalWaste * L0 * decayRate;

            dto.EstimatedDepth = wasteDepth;        // Height (Depth) of landfill
            dto.EstimatedDensity = wasteDensity;
            dto.EstimatedMSW = totalWaste;
            dto.MCF = wasteDepth >= 6.0 ? 0.8 : 0.5;
            dto.EstimatedVolume = dto.SurfaceArea * wasteDepth;
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

        private double GetSimpleDepth(double areaM2, string landfillType)
        {
            return landfillType.ToLowerInvariant() switch
            {
                "wild" or "illegal" => areaM2 switch
                {
                    <= 1_000 => 1.0,
                    <= 5_000 => 1.5,
                    <= 20_000 => 2.5,
                    _ => 3.5
                },

                "unsanitary" or "non_illegal" or "nonsanitary" => areaM2 switch
                {
                    <= 5_000 => 2.0,
                    <= 20_000 => 2.5,
                    <= 50_000 => 3.5,
                    _ => 4.5
                },

                "sanitary" => areaM2 switch
                {
                    <= 10_000 => 2.0,
                    <= 30_000 => 3.0,
                    <= 100_000 => 4.0,
                    <= 300_000 => 5.0,
                    _ => 6.0
                },

                _ => 2.0
            };
        }

        private double GetSimpleDensity(double areaM2, string landfillType)
        {
            return landfillType.ToLowerInvariant() switch
            {
                "wild" or "illegal" => areaM2 switch
                {
                    <= 1_000 => 0.50,
                    <= 5_000 => 0.55,
                    _ => 0.60
                },

                "unsanitary" or "non_illegal" or "nonsanitary" => areaM2 switch
                {
                    <= 5_000 => 0.60,
                    <= 20_000 => 0.65,
                    <= 50_000 => 0.65,
                    _ => 0.70
                },

                "sanitary" => areaM2 switch
                {
                    <= 10_000 => 0.70,
                    <= 30_000 => 0.75,
                    <= 100_000 => 0.75,
                    <= 300_000 => 0.80,
                    _ => 0.80
                },

                _ => 0.7
            };
        }

        private double GetRegionDecayRate(SerbianRegion? region)
        {
            return region switch
            {
                SerbianRegion.Vojvodina => 0.065,         // Warmer region
                SerbianRegion.Belgrade => 0.06,           // Urban area
                SerbianRegion.WesternSerbia => 0.055,     // Cooler mountains
                SerbianRegion.EasternSerbia => 0.055,     // Cooler mountains
                SerbianRegion.SouthernSerbia => 0.058,    // Moderate
                SerbianRegion.SumadijaPomoravlje => 0.06, // Moderate
                SerbianRegion.KosovoMetohija => 0.058,    // Moderate
                _ => K_DEFAULT                            // Default
            };
        }
    }
}
