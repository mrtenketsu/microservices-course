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
    }
}