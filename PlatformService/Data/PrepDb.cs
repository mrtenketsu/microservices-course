using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Model;

namespace PlatformService.Data;

[SuppressMessage("ReSharper", "InvertIf")]
public class PrepDb
{
    public static void PrepPopulation(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var logger = serviceScope.ServiceProvider.GetService<ILogger<PrepDb>>();
        var dbContext = serviceScope.ServiceProvider.GetService<AppDbContext>();
        SeedData(dbContext, logger);
    }

    private static void SeedData(AppDbContext context, ILogger logger)
    {
        if (context.Database.IsSqlServer())
        {
            logger.LogInformation("Attempting to apply migrations...");
            try
            {
                context.Database.Migrate();
                logger.LogInformation("Migrations applied");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not run migrations: {Message}", ex.Message);
            }
        }

        if (!context.Platforms.Any())
        {
            logger.LogInformation("Seeding platforms...");
            
            context.Platforms.AddRange(
                new Platform {Name = "DotNet", Publisher = "Microsoft", Cost = "Free"},
                new Platform {Name = "SQL Server Express", Publisher = "Microsoft", Cost = "Free"},
                new Platform {Name = "Kubernetes", Publisher = "Cloud Native Computing Foundation", Cost = "Free"}
            );

            context.SaveChanges();
            
            logger.LogInformation("Seeding platforms complete");
        }
    }
}