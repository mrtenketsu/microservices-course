using Microsoft.Extensions.Hosting;

namespace MessageBus;

public class MessageBusService : BackgroundService
{
    private readonly IMessageBusListener messageBusListener;

    public MessageBusService(IMessageBusListener messageBusListener)
    {
        this.messageBusListener = messageBusListener;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("--> Message bus service - execute");
        messageBusListener.StartReceive(stoppingToken);

        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("--> Message bus service - start");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("--> Message bus service - stop");
        
        return base.StopAsync(cancellationToken);
    }
}