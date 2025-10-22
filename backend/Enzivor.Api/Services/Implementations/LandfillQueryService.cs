using AutoMapper;
using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos.Landfills;
using Enzivor.Api.Repositories.Interfaces;
using Enzivor.Api.Services.Interfaces;

namespace Enzivor.Api.Services.Implementations
{
    /// <summary>
    /// Provides read-only operations for retrieving landfill data.
    /// </summary>
    public sealed class LandfillQueryService : ILandfillQueryService
    {
        private readonly ILandfillSiteRepository _repo;
        private readonly IMapper _mapper;

        public LandfillQueryService(ILandfillSiteRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LandfillSite>> GetAllLandfillsAsync(CancellationToken ct)
        {
            var sites = await _repo.GetAllAsync(ct);
            return _mapper.Map<IEnumerable<LandfillSite>>(sites);
        }

        public async Task<LandfillSite?> GetLandfillByIdAsync(int id, CancellationToken ct)
        {
            var site = await _repo.GetByIdAsync(id, ct);
            return site is null ? null : _mapper.Map<LandfillSite>(site);
        }

        public async Task<IEnumerable<LandfillSite>> GetLandfillsByRegionAsync(string regionKey, CancellationToken ct)
        {
            var canonicalKey = Normalize(regionKey);
            if (string.IsNullOrWhiteSpace(canonicalKey))
                return Enumerable.Empty<LandfillSite>();

            var displayName = canonicalKey switch
            {
                "vojvodina" => "Vojvodina",
                "beograd" => "Beograd",
                "zapadnasrbija" => "Zapadna Srbija",
                "sumadijaipomoravlje" => "Šumadija i Pomoravlje",
                "istocnasrbija" => "Istočna Srbija",
                "juznasrbija" => "Južna Srbija",
                _ => canonicalKey
            };

            var sites = await _repo.GetByRegionAsync(displayName, ct);
            var dtos = _mapper.Map<List<LandfillSite>>(sites);

            // preserve canonical key for FE
            foreach (var dto in dtos)
                dto.RegionTag = canonicalKey;

            return dtos;
        }

        private static string Normalize(string? s) =>
            string.IsNullOrWhiteSpace(s)
                ? ""
                : new string(s.Trim().ToLowerInvariant().Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
}