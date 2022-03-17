using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http;

public class HttpCommandDataClient : ICommandDataClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<HttpCommandDataClient> logger;

    public HttpCommandDataClient(HttpClient httpClient, ILogger<HttpCommandDataClient> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    public async Task SendPlatformToCommand(PlatformReadDto plat)
    {
        var httpContent = new StringContent(
            JsonSerializer.Serialize(plat),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync(Urls.CommandService.PostPlatforms(), httpContent);

        if (response.IsSuccessStatusCode)
            logger.LogInformation("Sync POST to CommandService was OK");
        else
            logger.LogWarning("Sync POST to CommandService was NOT OK, status code {StatusCode}", response.StatusCode);
    }
}