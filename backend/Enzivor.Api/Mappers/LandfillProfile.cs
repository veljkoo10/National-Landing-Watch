using AutoMapper;
using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Dtos.Landfills;

namespace Enzivor.Api.Mapping
{
    /// <summary>
    /// Defines AutoMapper profiles for all Landfill-related models and DTOs.
    /// Includes bi-directional mappings for creation and presentation.
    /// </summary>
    public class LandfillProfile : Profile
    {
        public LandfillProfile()
        {
            // LandfillSite <=> ShowLandfillDto
            CreateMap<LandfillSite, ShowLandfillDto>()
                .ForMember(dest => dest.RegionKey, opt => opt.MapFrom(src => src.RegionTag ?? "unknown"))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.PointLat ?? 0))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.PointLon ?? 0))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => GetFrontendType(src.Category.ToString())))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => GetSizeFromArea(src.EstimatedAreaM2)))
                .ForMember(dest => dest.YearCreated, opt => opt.MapFrom(src => src.StartYear ?? 2020))
                .ForMember(dest => dest.AreaM2, opt => opt.MapFrom(src =>
                    src.EstimatedAreaM2.HasValue ? Math.Round(src.EstimatedAreaM2.Value, 2) : (double?)null));

            CreateMap<ShowLandfillDto, LandfillSite>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.RegionTag, opt => opt.MapFrom(src => src.RegionKey))
                .ForMember(dest => dest.PointLat, opt => opt.MapFrom(src => src.Latitude))
                .ForMember(dest => dest.PointLon, opt => opt.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.EstimatedAreaM2, opt => opt.MapFrom(src => src.AreaM2))
                .ForMember(dest => dest.StartYear, opt => opt.MapFrom(src => src.YearCreated))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Region, opt => opt.Ignore())
                .ForMember(dest => dest.Detections, opt => opt.Ignore());

            // LandfillDetection <=> LandfillDto
            CreateMap<LandfillDetection, LandfillDto>()
                .ForMember(dest => dest.ImageName, opt => opt.MapFrom(src => src.ImageName))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Confidence, opt => opt.MapFrom(src => src.Confidence))
                .ForMember(dest => dest.PolygonCoordinates, opt => opt.MapFrom(src => src.PolygonCoordinates))
                .ForMember(dest => dest.NorthWestLat, opt => opt.MapFrom(src => src.NorthWestLat))
                .ForMember(dest => dest.NorthWestLon, opt => opt.MapFrom(src => src.NorthWestLon))
                .ForMember(dest => dest.SouthEastLat, opt => opt.MapFrom(src => src.SouthEastLat))
                .ForMember(dest => dest.SouthEastLon, opt => opt.MapFrom(src => src.SouthEastLon))
                .ForMember(dest => dest.SurfaceArea, opt => opt.MapFrom(src => src.SurfaceArea))
                .ForMember(dest => dest.RegionTag, opt => opt.MapFrom(src => src.RegionTag))
                .ForMember(dest => dest.ParsedRegion, opt => opt.MapFrom(src => src.Region))
                .ForMember(dest => dest.KnownLandfillName, opt => opt.MapFrom(src => src.LandfillName))
                .ForMember(dest => dest.CenterLat, opt => opt.Ignore())
                .ForMember(dest => dest.CenterLon, opt => opt.Ignore())
                .ForMember(dest => dest.ZoomLevel, opt => opt.Ignore());

            CreateMap<LandfillDto, LandfillDetection>()
                .ForMember(dest => dest.ImageName, opt => opt.MapFrom(src => src.ImageName))
                .ForMember(dest => dest.LandfillName, opt => opt.MapFrom(src => src.KnownLandfillName))
                .ForMember(dest => dest.Type, opt => opt.Ignore()) // requires enum parse
                .ForMember(dest => dest.Confidence, opt => opt.MapFrom(src => src.Confidence))
                .ForMember(dest => dest.PolygonCoordinates, opt => opt.MapFrom(src => src.PolygonCoordinates))
                .ForMember(dest => dest.NorthWestLat, opt => opt.MapFrom(src => src.NorthWestLat))
                .ForMember(dest => dest.NorthWestLon, opt => opt.MapFrom(src => src.NorthWestLon))
                .ForMember(dest => dest.SouthEastLat, opt => opt.MapFrom(src => src.SouthEastLat))
                .ForMember(dest => dest.SouthEastLon, opt => opt.MapFrom(src => src.SouthEastLon))
                .ForMember(dest => dest.SurfaceArea, opt => opt.MapFrom(src => src.SurfaceArea))
                .ForMember(dest => dest.RegionTag, opt => opt.MapFrom(src => src.RegionTag))
                .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.ParsedRegion))
                .ForMember(dest => dest.LandfillSite, opt => opt.Ignore())
                .ForMember(dest => dest.LandfillSiteId, opt => opt.Ignore());

            // LandfillSite <=> LandfillDto
            CreateMap<LandfillSite, LandfillDto>()
                .ForMember(dest => dest.KnownLandfillName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.RegionTag, opt => opt.MapFrom(src => src.RegionTag))
                .ForMember(dest => dest.SurfaceArea, opt => opt.MapFrom(src => src.EstimatedAreaM2 ?? 0))
                .ForMember(dest => dest.CenterLat, opt => opt.MapFrom(src => src.PointLat ?? 0))
                .ForMember(dest => dest.CenterLon, opt => opt.MapFrom(src => src.PointLon ?? 0))
                .ForMember(dest => dest.ParsedRegion, opt => opt.MapFrom(src => src.Region))
                .ForMember(dest => dest.EstimatedDepth, opt => opt.MapFrom(src => src.EstimatedDepth ?? 0))
                .ForMember(dest => dest.EstimatedDensity, opt => opt.MapFrom(src => src.EstimatedDensity ?? 0))
                .ForMember(dest => dest.EstimatedMSW, opt => opt.MapFrom(src => src.EstimatedMSW ?? 0))
                .ForMember(dest => dest.MCF, opt => opt.MapFrom(src => src.MCF ?? 0))
                .ForMember(dest => dest.EstimatedVolume, opt => opt.MapFrom(src => src.EstimatedVolumeM3 ?? 0))
                .ForMember(dest => dest.CH4GeneratedTonnes, opt => opt.MapFrom(src => src.EstimatedCH4Tons ?? 0))
                .ForMember(dest => dest.CO2EquivalentTonnes, opt => opt.MapFrom(src => src.EstimatedCO2eTons ?? 0));

            CreateMap<LandfillDto, LandfillSite>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.KnownLandfillName))
                .ForMember(dest => dest.RegionTag, opt => opt.MapFrom(src => src.RegionTag))
                .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.ParsedRegion))
                .ForMember(dest => dest.EstimatedAreaM2, opt => opt.MapFrom(src => (double?)src.SurfaceArea))
                .ForMember(dest => dest.PointLat, opt => opt.MapFrom(src => src.CenterLat))
                .ForMember(dest => dest.PointLon, opt => opt.MapFrom(src => src.CenterLon))
                .ForMember(dest => dest.EstimatedDepth, opt => opt.MapFrom(src => (double?)src.EstimatedDepth))
                .ForMember(dest => dest.EstimatedDensity, opt => opt.MapFrom(src => (double?)src.EstimatedDensity))
                .ForMember(dest => dest.EstimatedMSW, opt => opt.MapFrom(src => (double?)src.EstimatedMSW))
                .ForMember(dest => dest.MCF, opt => opt.MapFrom(src => (double?)src.MCF))
                .ForMember(dest => dest.EstimatedVolumeM3, opt => opt.MapFrom(src => (double?)src.EstimatedVolume))
                .ForMember(dest => dest.EstimatedCH4Tons, opt => opt.MapFrom(src => (double?)src.CH4GeneratedTonnes))
                .ForMember(dest => dest.EstimatedCO2eTons, opt => opt.MapFrom(src => (double?)src.CO2EquivalentTonnes))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Detections, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }

        // Helper methods for computed values
        private static string GetFrontendType(string? category)
        {
            switch ((category ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "illegal": return "wild";
                case "nonsanitary": return "unsanitary";
                case "sanitary": return "sanitary";
                default: return "unsanitary";
            }
        }

        private static string GetSizeFromArea(double? areaM2)
        {
            if (!areaM2.HasValue) return "small";
            var a = areaM2.Value;
            if (a <= 10_000) return "small";
            if (a <= 50_000) return "medium";
            return "large";
        }
    }
}