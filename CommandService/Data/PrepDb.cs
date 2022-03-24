using System.Diagnostics.CodeAnalysis;
using CommandService.SyncDataServices.Grpc;
using Microsoft.EntityFrameworkCore;

namespace CommandService.Data;

[SuppressMessage("ReSharper", "InvertIf")]
public class PrepDb
{
    public static void PrepPopulation(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        SeedData(
            serviceScope.ServiceProvider.GetService<AppDbContext>(),
            serviceScope.ServiceProvider.GetService<ILogger<PrepDb>>(),
            serviceScope.ServiceProvider.GetService<IPlatformDataClient>(),
            serviceScope.ServiceProvider.GetService<IPlatformRepo>());
    }

    private static void SeedData(AppDbContext context, ILogger logger, IPlatformDataClient platformdataClient, IPlatformRepo platformRepo)
    {
        if (context.Database.IsSqlServer())
        {
            logger.LogInformation("Attempting to apply migrations...");
            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not run migrations: {Message}", ex.Message);
            }
        }
        
        var platformsSequence = platformdataClient.ReturnAllPlatforms();
        try
        {
            foreach (var platform in platformsSequence)
            {
                if (!platformRepo.ExternalPlatformExist(platform.ExternalId))
                {
                    logger.LogInformation("Creating platform name {Name}, external id {ExternalId}", platform.Name, platform.ExternalId);
                    platformRepo.CratePlatform(platform);
                }
            }

            logger.LogInformation("Saving imported platforms");
            platformRepo.SaveChanges();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error importing platforms: {Message}", ex.Message);
        }
        
        
    }
}