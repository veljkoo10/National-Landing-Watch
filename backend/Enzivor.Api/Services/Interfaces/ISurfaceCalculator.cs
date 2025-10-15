using Enzivor.Api.Models.Dtos;

namespace Enzivor.Api.Services.Interfaces
{
    public interface ISurfaceCalculator
    {
        double CalculateSurfaceArea(LandfillDto dto);
    }
}
