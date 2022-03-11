using AutoMapper;
using PlatformService.Dtos;
using PlatformService.Model;

namespace PlatformService;

public class PlatformsProfile : Profile
{
    public PlatformsProfile()
    {
        // Source -> Target
        CreateMap<Platform, PlatformReadDto>();
        CreateMap<Platform, PlatformPublishedDto>();
        CreateMap<PlatformCreateDto, Platform>();
    }
}