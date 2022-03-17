using AutoMapper;
using MessageBus;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Model;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlatformsController : ControllerBase
{
    private readonly ICommandDataClient commandDataClient;
    private readonly IMessageBusPublisher messageBusPublisher;
    private readonly ILogger<PlatformsController> logger;
    private readonly IMapper mapper;
    private readonly IPlatformRepo platformRepo;

    public PlatformsController(
        IPlatformRepo platformRepo,
        IMapper mapper,
        ICommandDataClient commandDataClient,
        IMessageBusPublisher messageBusPublisher,
        ILogger<PlatformsController> logger)
    {
        this.platformRepo = platformRepo;
        this.mapper = mapper;
        this.commandDataClient = commandDataClient;
        this.messageBusPublisher = messageBusPublisher;
        this.logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
    {
        var platforms = platformRepo.GetAllPlatforms();
        var dtos = mapper.Map<IEnumerable<PlatformReadDto>>(platforms);

        return Ok(dtos);
    }

    [HttpGet("{id:int}", Name = "GetPlatformById")]
    public ActionResult<PlatformReadDto> GetPlatformById(int id)
    {
        var platform = platformRepo.GetPlatformById(id);
        if (platform != null)
        {
            var dto = mapper.Map<PlatformReadDto>(platform);
            return Ok(dto);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<PlatformReadDto>> CreatePlatform([FromBody] PlatformCreateDto createDto)
    {
        var platform = mapper.Map<Platform>(createDto);
        platformRepo.CreatePlatform(platform);
        platformRepo.SaveChanges();

        var readDto = mapper.Map<PlatformReadDto>(platform);
        
        try
        {
            await commandDataClient.SendPlatformToCommand(readDto);
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not send synchronously: {Message}", ex.Message);
        }
        
        try
        {
            var publishDto = mapper.Map<PlatformPublishedDto>(platform);
            messageBusPublisher.Publish(publishDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not send asynchronously: {Message}", ex.Message);
        }

        return CreatedAtRoute(nameof(GetPlatformById), new {platform.Id}, readDto);
    }
}