using Enzivor.Api.Models.Domain;

namespace Enzivor.Api.Repositories.Interfaces
{
    public interface ILandfillDetectionRepository
    {
        Task<List<LandfillDetection>> GetAllAsync(CancellationToken ct = default);
        Task<List<LandfillDetection>> GetUnlinkedAsync(CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<LandfillDetection> detections, CancellationToken ct = default);
        Task<LandfillDetection?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
