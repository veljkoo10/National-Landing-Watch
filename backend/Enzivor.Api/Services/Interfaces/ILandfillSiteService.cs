using Enzivor.Api.Models.Enums;

namespace Enzivor.Api.Services.Interfaces
{
    public interface ILandfillSiteService
    {
        Task<int> CreateSitesFromUnlinkedDetectionsAsync(
           double? minConfidence = null,
           LandfillCategory? category = null,
           CancellationToken ct = default);
    }
}
