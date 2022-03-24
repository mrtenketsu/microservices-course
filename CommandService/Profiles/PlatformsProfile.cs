using AutoMapper;
using CommandService.Dtos;
using CommandService.Models;

namespace CommandService.Profiles;

public class PlatformsProfile : Profile
{
    public PlatformsProfile()
    {
        // Source -> Target
        CreateMap<Platform, PlatformReadDto>();
        CreateMap<PlatformCreateDto, Platform>();
        CreateMap<PlatformPublishedDto, Platform>()
            .ForMember(dst => dst.ExternalId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.Id, opt => opt.Ignore());
        CreateMap<GrpcPlatformModel, Platform>()
            .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.PlatformId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dst => dst.Commands, opt => opt.Ignore())
            .ForMember(dst => dst.Id, opt => opt.Ignore());
    }
}