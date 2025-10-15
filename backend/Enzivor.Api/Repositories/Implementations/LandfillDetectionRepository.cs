using Enzivor.Api.Data;
using Enzivor.Api.Models.Domain;
using Enzivor.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enzivor.Api.Repositories.Implementations
{
    public class LandfillDetectionRepository : ILandfillDetectionRepository
    {
        private readonly AppDbContext _db;

        public LandfillDetectionRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<LandfillDetection>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.LandfillDetections.ToListAsync(ct);
        }

        public async Task<List<LandfillDetection>> GetUnlinkedAsync(CancellationToken ct = default)
        {
            return await _db.LandfillDetections
                .Where(d => d.LandfillSiteId == null)
                .ToListAsync(ct);
        }

        public async Task AddRangeAsync(IEnumerable<LandfillDetection> detections, CancellationToken ct = default)
        {
            await _db.LandfillDetections.AddRangeAsync(detections, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<LandfillDetection?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.LandfillDetections
                .Include(d => d.LandfillSite)
                .FirstOrDefaultAsync(d => d.Id == id, ct);
        }
    }
}
