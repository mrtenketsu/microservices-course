using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using MessageBus;

namespace CommandService.MessageHandlers;

public class PlatformPublishedHandler : IMessageHandler<PlatformPublishedDto>
{
    private readonly IPlatformRepo platformRepo;
    private readonly IMapper mapper;

    public PlatformPublishedHandler(IPlatformRepo platformRepo, IMapper mapper)
    {
        this.platformRepo = platformRepo;
        this.mapper = mapper;
    }
    
    public Task HandleMessage(PlatformPublishedDto dto)
    {
        if (platformRepo.ExternalPlatformExist(dto.Id))
            return Task.CompletedTask;
        
        var plat = mapper.Map<Platform>(dto);
        platformRepo.CratePlatform(plat);

        var saved = platformRepo.SaveChanges();

        if (saved)
            Console.WriteLine("--> Created platform, name {0}, external id {1}, id {2}", plat.Name, plat.ExternalId, plat.Id);
        else
            Console.WriteLine("--> Failed to create a platform, name {0}, external id {1}", plat.Name, plat.ExternalId);

        return Task.CompletedTask;
    }
}