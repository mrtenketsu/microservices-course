using System.Text;
using System.Text.Json;
using PlatformService.Settings;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices;

public class RabbitMqMessageBusClient : IMessageBusClient, IDisposable
{
    private readonly RabbitMqMessageBusSettings settings;
    private readonly IConnection connection;
    private readonly IModel channel;

    public RabbitMqMessageBusClient(RabbitMqMessageBusSettings settings)
    {
        this.settings = settings;

        Console.WriteLine($"--> RabbitMq message bus client, attempting to connect {settings.Host}:{settings.Port}");
        
        var factory = new ConnectionFactory()
        {
            HostName = settings.Host,
            Port = settings.Port
        };

        try
        {
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: settings.Exchange, type: settings.ExchangeType);
            
            connection.ConnectionShutdown += OnConnectionShutdown;
            
            Console.WriteLine($"--> Connected to RabbitMq message bus");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not connect to RabbitMq message bus: {ex.Message}");
        }
    }

    private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine($"--> RabbitMq message bus connection shutdown, cause: {e.Cause}");
    }

    public void Publish(object dto)
    {
        var messageType = dto.GetType().Name;
        var message = JsonSerializer.Serialize(dto);
        
        if (connection.IsOpen)
        {
            Console.WriteLine("--> RabbitMq message bus connection is open, sending message...");
            SendMessage(message, messageType);
        }
        else
        {
            Console.WriteLine("--> RabbitMq message bus connection is closed, not sending message");
        }
    }

    private void SendMessage(string message, string messageType)
    {
        var body = Encoding.UTF8.GetBytes(message);

        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        
        channel.BasicPublish(settings.Exchange, messageType, props, body);
        
        Console.WriteLine("--> RabbitMq message bus message sent");
    }

    public void Dispose()
    {
        Console.WriteLine("--> RabbitMq message bus client disposing");

        if (channel?.IsOpen == true)
        {
            channel.Close();
            connection.Close();
        }
        
        channel?.Dispose();
        connection?.Dispose();
        
        Console.WriteLine("--> RabbitMq message bus client disposed");
    }
}