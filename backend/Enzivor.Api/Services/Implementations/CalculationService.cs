using Enzivor.Api.Models.Dtos.Landfills;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    /// <summary>
    /// Provides landfill-related physical and emission calculations
    /// using the IPCC first-order decay model.
    /// </summary>
    public class CalculationService : ICalculationService
    {
        private const double L0 = 0.1;              // Methane generation potential (m³ CH₄ per tonne of waste)
        private const double CH4_TO_CO2 = 25.0;     // Global warming potential (CH₄ → CO₂e)

        public void CalculateMethaneEmissions(LandfillDto dto)
        {
            if (dto?.SurfaceArea <= 0 || !dto.StartYear.HasValue)
                return;

            var currentYear = DateTime.UtcNow.Year;
            var startYear = dto.StartYear.Value;
            if (startYear >= currentYear) return;

            var yearsActive = currentYear - startYear;

            var wasteDepth = GetSimpleDepth(dto.SurfaceArea, dto.Type);
            var wasteDensity = GetSimpleDensity(dto.SurfaceArea, dto.Type);
            var totalWaste = dto.SurfaceArea * wasteDepth * wasteDensity;
            var decayRate = GetRegionDecayRate(dto.ParsedRegion);

            dto.EstimatedDepth = wasteDepth;
            dto.EstimatedDensity = wasteDensity;
            dto.EstimatedMSW = totalWaste;
            dto.MCF = wasteDepth >= 6.0 ? 0.8 : 0.5;
            dto.EstimatedVolume = dto.SurfaceArea * wasteDepth;

            double methaneSum = 0;

            // Skip first year (methane starts after 1 year)
            for (int i = 1; i < yearsActive; i++)
            {
                methaneSum += totalWaste * L0 * decayRate * Math.Exp(-decayRate * (yearsActive - i)) * dto.MCF;
            }

            dto.CH4GeneratedTonnes = methaneSum;
            dto.CO2EquivalentTonnes = methaneSum * CH4_TO_CO2;
        }

        public void CalculateMethaneEmissions(IEnumerable<LandfillDto> landfills)
        {
            foreach (var landfill in landfills)
                CalculateMethaneEmissions(landfill);
        }

        public double GetMethaneGenerationPotential() => L0;

        // Helpers
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
            // warmer areas -> higher decay rate
            // cooler areas -> lower decay rate
            return region switch
            {
                SerbianRegion.Vojvodina => 0.065,
                SerbianRegion.Belgrade => 0.06,
                SerbianRegion.WesternSerbia => 0.055,
                SerbianRegion.EasternSerbia => 0.055,
                SerbianRegion.SouthernSerbia => 0.058,
                SerbianRegion.SumadijaPomoravlje => 0.06,
                SerbianRegion.KosovoMetohija => 0.058,
                _ => 0.06
            };
        }
    }
}