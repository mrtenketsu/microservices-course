using System;
using AutoMapper;
using CommandService.Data;
using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers;

[Route("api/c/[Controller]")]
[ApiController]
public class PlatformsController : ControllerBase
{
    private readonly ICommandRepo commandRepo;
    private readonly IPlatformRepo platformRepo;
    private readonly IMapper mapper;

    public PlatformsController(
        ICommandRepo commandRepo,
        IPlatformRepo platformRepo,
        IMapper mapper)
    {
        this.commandRepo = commandRepo;
        this.platformRepo = platformRepo;
        this.mapper = mapper;
    }
    
    [HttpPost]
    public ActionResult TestInboundConnection()
    {
        Console.WriteLine("--> Inbound POST # Command Service");

        return Ok("Inbound test of from Platforms Controller");
    }
}