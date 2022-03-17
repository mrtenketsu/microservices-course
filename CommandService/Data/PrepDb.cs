using System.Diagnostics.CodeAnalysis;
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
            serviceScope.ServiceProvider.GetService<ILogger<PrepDb>>());
    }

    private static void SeedData(AppDbContext context, ILogger logger)
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
    }
}