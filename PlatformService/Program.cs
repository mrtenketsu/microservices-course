using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Settings;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddSingleton(sp => sp.GetSettings<UrlSettings>());
services.AddSingleton(sp => sp.GetSettings<RabbitMqMessageBusSettings>());


// Add services to the container.
services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

const string DATABASE_CONNECTION_STRING_NAME = "PlatformsConn";
services.AddDbContext<AppDbContext>((sp, opt) =>
{
    var configuration = sp.GetService<IConfiguration>();
    var connectionString = configuration.GetConnectionString(DATABASE_CONNECTION_STRING_NAME);
    opt.UseSqlServer(connectionString);
});

services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>((sp, cnf) =>
{
    var urlSettings = sp.GetService<UrlSettings>();
    cnf.BaseAddress = new Uri(urlSettings.CommandServiceBaseUrl);
});

services.AddSingleton<IMessageBusClient, RabbitMqMessageBusClient>();

services.AddScoped<IPlatformRepo, PlatformRepo>();

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

PrepDb.PrepPopulation(app);

app.Run();


[SuppressMessage("ReSharper", "HeapView.PossibleBoxingAllocation")]
public static class StartupExtensions
{
    public static TSettings GetSettings<TSettings>(this IServiceProvider sp)
        where TSettings : new()
    {
        var configuration = sp.GetService<IConfiguration>();

        TSettings settings = new();
        var settingName = settings.GetType().Name;

        configuration.Bind(settingName, settings);

        return settings;
    }
}