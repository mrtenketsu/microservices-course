using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers;

[Route("api/c/platforms/{platformId:int}/[Controller]")]
[ApiController]
public class CommandsController : ControllerBase
{
    private readonly ICommandRepo commandRepo;
    private readonly IPlatformRepo platformRepo;
    private readonly IMapper mapper;
    private readonly ILogger<CommandsController> logger;

    public CommandsController(
        ICommandRepo commandRepo,
        IPlatformRepo platformRepo,
        IMapper mapper,
        ILogger<CommandsController> logger)
    {
        this.commandRepo = commandRepo;
        this.platformRepo = platformRepo;
        this.mapper = mapper;
        this.logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
    {
        logger.LogDebug("Getting commands for platform {platformId}", platformId);

        if (!platformRepo.PlatformExist(platformId))
            return NotFound();

        var commands = commandRepo.GetCommandsForPlatforms(platformId);
        var dtos = mapper.Map<IEnumerable<CommandReadDto>>(commands);

        return Ok(dtos);
    }

    [HttpGet("{commandId:int}", Name = "GetCommandForPlatform")]
    public ActionResult<CommandReadDto> GetCommandForPlatform(
        int platformId, int commandId)
    {
        logger.LogDebug("Getting command {commandId} for platform {platformId}", commandId, platformId);

        if (!platformRepo.PlatformExist(platformId))
            return NotFound();

        var command = commandRepo.GetCommand(platformId, commandId);
        if (command == null)
            return NotFound();
        
        var readDto = mapper.Map<CommandReadDto>(command);
        
        return Ok(readDto);
    }

    [HttpPost]
    public ActionResult<CommandReadDto> CreateCommand(
        int platformId,
        [FromBody] CommandCreateDto createDto)
    {
        logger.LogInformation("Creating command for platform {platformId}", platformId);

        if (!platformRepo.PlatformExist(platformId))
            return NotFound();

        var command = mapper.Map<Command>(createDto);
        commandRepo.CreateCommand(platformId, command);
        commandRepo.SaveChanges();

        var readDto = mapper.Map<CommandReadDto>(command);
        
        return CreatedAtRoute(
            nameof(GetCommandForPlatform),
            new { platformId, commandId = command.Id },
            readDto);
    }
}