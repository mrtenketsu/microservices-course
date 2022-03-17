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
    private readonly ILogger<PlatformPublishedHandler> logger;

    public PlatformPublishedHandler(IPlatformRepo platformRepo, IMapper mapper, ILogger<PlatformPublishedHandler> logger)
    {
        this.platformRepo = platformRepo;
        this.mapper = mapper;
        this.logger = logger;
    }
    
    public Task HandleMessage(PlatformPublishedDto dto)
    {
        if (platformRepo.ExternalPlatformExist(dto.Id))
            return Task.CompletedTask;
        
        var plat = mapper.Map<Platform>(dto);
        platformRepo.CratePlatform(plat);

        var saved = platformRepo.SaveChanges();

        if (saved)
            logger.LogInformation("Created platform, name {Name}, external id {ExternalId}, id {Id}", plat.Name, plat.ExternalId, plat.Id);
        else
            logger.LogWarning("Failed to create a platform, name {Name}, external id {ExternalId}", plat.Name, plat.ExternalId);

        return Task.CompletedTask;
    }
}