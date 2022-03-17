using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageBus;

public class MessageBusService : BackgroundService
{
    private readonly IMessageBusListener messageBusListener;
    private readonly ILogger<MessageBusService> logger;

    public MessageBusService(IMessageBusListener messageBusListener, ILogger<MessageBusService> logger)
    {
        this.messageBusListener = messageBusListener;
        this.logger = logger;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Message bus service - execute");
        messageBusListener.StartReceive(stoppingToken);

        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Message bus service - start");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Message bus service - stop");
        
        return base.StopAsync(cancellationToken);
    }
}