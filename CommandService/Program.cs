using System.Diagnostics.CodeAnalysis;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.MessageHandlers;
using CommandService.Settings;
using CommandService.SyncDataServices.Grpc;
using MessageBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.

// Settings
services.AddSingleton<IRabbitMqMessageBusSettings>(sp => sp.GetSettings<RabbitMqMessageBusSettings>());
services.AddSingleton(sp => sp.GetSettings<GrpcSettings>());

// Message bus
services.AddSingleton<IMessageBusSubscriptionManager, MessageBusSubscriptionsManager>();
services.AddSingleton<IMessageBusListener, RabbitMqMessageBusListener>();
services.AddHostedService<MessageBusService>();

// Message bus message handlers
services.AddScoped<PlatformPublishedHandler>();

// Database and repositories
const string DATABASE_CONNECTION_STRING_NAME = "CommandsConn";
services.AddDbContext<AppDbContext>((sp, opt) =>
{
    var configuration = sp.GetService<IConfiguration>();
    var connectionString = configuration.GetConnectionString(DATABASE_CONNECTION_STRING_NAME);

    opt.UseSqlServer(connectionString);
});

services.AddScoped<IPlatformRepo, PlatformRepo>();
services.AddScoped<ICommandRepo, CommandRepo>();
services.AddScoped<IPlatformDataClient, PlatformDataClient>();

// ASP.NET stuff
services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// AutoMapper
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Host.UseNLog();

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

// Configure message bus handlers
var messageBusListener = app.Services.GetService<IMessageBusListener>();
messageBusListener.Subscribe<PlatformPublishedHandler, PlatformPublishedDto>();

// run the app
app.Run();

public static class StartupExtensions
{
    [SuppressMessage("ReSharper", "HeapView.PossibleBoxingAllocation")]
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