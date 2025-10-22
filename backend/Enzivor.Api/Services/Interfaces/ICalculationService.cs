using Enzivor.Api.Models.Dtos.Landfills;
using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Services.Interfaces
{
    /// <summary>
    /// Defines methods for calculating methane and CO₂ emissions from landfill data.
    /// </summary>
    public interface ICalculationService
    {
        /// <summary>
        /// Calculates all landfill parameters (volume, waste, CH₄, CO₂e, etc.)
        /// for the given landfill DTO.
        /// </summary>
        void CalculateMethaneEmissions(LandfillDto dto);

        /// <summary>
        /// Calculates all landfill parameters (volume, waste, CH₄, CO₂e, etc.)
        /// for the given landfill Dtos.
        /// </summary>
        void CalculateMethaneEmissions(IEnumerable<LandfillDto> landfills);

        /// <summary>
        /// Returns the methane generation potential (L₀ constant).
        /// </summary>
        double GetMethaneGenerationPotential();
    }
}