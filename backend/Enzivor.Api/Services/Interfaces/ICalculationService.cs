using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Services.Interfaces
{
    public interface ICalculationService
    {
        void CalculateMethaneEmissions(LandfillDto dto);
        void CalculateMethaneEmissions(IEnumerable<LandfillDto> landfills);
        double GetMethaneGenerationPotential();
    }
}
