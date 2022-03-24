using AutoMapper;
using CommandService.Models;
using CommandService.Settings;
using Grpc.Net.Client;

namespace CommandService.SyncDataServices.Grpc;

public class PlatformDataClient: IPlatformDataClient
{
    private readonly IMapper mapper;
    private readonly GrpcSettings grpcSettings;
    private readonly ILogger<PlatformDataClient> logger;

    public PlatformDataClient(IMapper mapper, GrpcSettings grpcSettings, ILogger<PlatformDataClient> logger)
    {
        this.mapper = mapper;
        this.grpcSettings = grpcSettings;
        this.logger = logger;
    }
    
    public IEnumerable<Platform> ReturnAllPlatforms()
    {
        logger.LogInformation("Calling GRPC service to get all platforms data, url: {PlatformsServiceUrl}", grpcSettings.PlatformsServiceUrl);

        var channel = GrpcChannel.ForAddress(grpcSettings.PlatformsServiceUrl);
        var client = new GrpcPlatform.GrpcPlatformClient(channel);
        var request = new GetAllRequests();

        try
        {
            var reply = client.GetAllPlatforms(request);
            return reply.Platforms.Select(mapper.Map<Platform>);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling GRPC service: {Message}", ex.Message);
        }

        return Enumerable.Empty<Platform>();
    }
}