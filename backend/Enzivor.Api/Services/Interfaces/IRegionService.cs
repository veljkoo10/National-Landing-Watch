using Enzivor.Api.Models.Dtos;

namespace Enzivor.Api.Services.Interfaces
{
    public interface IRegionService
    {
        Task<List<RegionDto>> GetAllRegionsAsync(CancellationToken ct = default);
        Task<RegionDto?> GetRegionByIdAsync(int id, CancellationToken ct = default);
        Task<RegionDto?> GetRegionByNameAsync(string name, CancellationToken ct = default);

    }
}
