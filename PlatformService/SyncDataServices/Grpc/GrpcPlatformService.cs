using AutoMapper;
using Grpc.Core;
using PlatformService.Data;

namespace PlatformService.SyncDataServices.Grpc;

public class GrpcPlatformService : GrpcPlatform.GrpcPlatformBase
{
    private readonly IPlatformRepo platformRepo;
    private readonly IMapper mapper;

    public GrpcPlatformService(IPlatformRepo platformRepo, IMapper mapper)
    {
        this.platformRepo = platformRepo;
        this.mapper = mapper;
    }

    public override Task<PlatformResponse> GetAllPlatforms(GetAllRequests request, ServerCallContext context)
    {
        var platforms = platformRepo.GetAllPlatforms();
        var response = new PlatformResponse();
        response
            .Platforms
            .AddRange(platforms.Select(p => mapper.Map<GrpcPlatformModel>(p)));

        return Task.FromResult(response);
    }
}