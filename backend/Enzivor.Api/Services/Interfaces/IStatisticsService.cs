using Enzivor.Api.Models.Dtos;

namespace Enzivor.Api.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<List<WasteByRegionDto>> GetTotalWasteByRegionAsync(CancellationToken ct = default);
        Task<List<LandfillTypeDto>> GetLandfillTypesAsync(CancellationToken ct = default);
        Task<Ch4OverTimeDto> GetCh4OverTimeAsync(CancellationToken ct = default);
        Task<List<TopLandfillDto>> GetTopLargestLandfillsAsync(int count = 3, CancellationToken ct = default);
        Task<MostImpactedRegionFullDto> GetMostImpactedRegionAsync(CancellationToken ct = default);
        Task<List<GrowthRowDto>> GetLandfillGrowthAsync(CancellationToken ct = default);
        Task<List<EmissionPerCapitaDto>> GetEmissionsPerCapitaAsync(CancellationToken ct = default);
    }
}