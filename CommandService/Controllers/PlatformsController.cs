using System;
using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers;

[Route("api/c/[Controller]")]
[ApiController]
public class PlatformsController : ControllerBase
{
    private readonly IPlatformRepo platformRepo;
    private readonly IMapper mapper;

    public PlatformsController(
        IPlatformRepo platformRepo,
        IMapper mapper)
    {
        this.platformRepo = platformRepo;
        this.mapper = mapper;
    }
    
    [HttpGet]
    public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
    {
        Console.WriteLine("--> Getting platforms from Command Service");
        
        var platforms = platformRepo.GetAllPlatforms();
        var dtos = mapper.Map<IEnumerable<PlatformReadDto>>(platforms);

        return Ok(dtos);
    }
    
    
    [HttpPost]
    public ActionResult TestInboundConnection()
    {
        Console.WriteLine("--> Inbound POST # Command Service");

        return Ok("Inbound test of from Platforms Controller");
    }
}