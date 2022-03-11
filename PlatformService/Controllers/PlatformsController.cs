using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
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
    private readonly IMessageBusClient messageBusClient;
    private readonly IMapper mapper;
    private readonly IPlatformRepo platformRepo;

    public PlatformsController(
        IPlatformRepo platformRepo,
        IMapper mapper,
        ICommandDataClient commandDataClient,
        IMessageBusClient messageBusClient)
    {
        this.platformRepo = platformRepo;
        this.mapper = mapper;
        this.commandDataClient = commandDataClient;
        this.messageBusClient = messageBusClient;
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
            Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
        }
        
        try
        {
            var publishDto = mapper.Map<PlatformPublishedDto>(platform);
            messageBusClient.Publish(publishDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
        }

        return CreatedAtRoute(nameof(GetPlatformById), new {platform.Id}, readDto);
    }
}