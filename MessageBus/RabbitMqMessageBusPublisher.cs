using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace MessageBus;

public class RabbitMqMessageBusPublisher : IMessageBusPublisher, IDisposable
{
    private readonly RabbitMqMessageBusSettings settings;
    private readonly IConnection connection;
    private readonly IModel channel;

    public RabbitMqMessageBusPublisher(RabbitMqMessageBusSettings settings)
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
    
    public void Publish(object message)
    {
        var messageType = message.GetType().Name;
        var messageJson = JsonSerializer.Serialize(message);
        
        if (connection.IsOpen)
        {
            Console.WriteLine("--> RabbitMq message bus connection is open, sending message...");
            PublishInternal(messageJson, messageType);
        }
        else
        {
            Console.WriteLine("--> RabbitMq message bus connection is closed, not sending message");
        }
    }
    
    private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine($"--> RabbitMq message bus connection shutdown, cause: {e.Cause}");
    }

    private void PublishInternal(string messageJson, string messageType)
    {
        var body = Encoding.UTF8.GetBytes(messageJson);

        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        
        channel.BasicPublish(settings.Exchange, messageType, props, body);
        
        Console.WriteLine("--> RabbitMq message bus message sent");
    }

    public void Dispose()
    {
        Console.WriteLine("--> RabbitMq message bus publisher disposing");

        if (channel?.IsOpen == true)
        {
            channel.Close();
            connection.Close();
        }
        
        channel?.Dispose();
        connection?.Dispose();
        
        Console.WriteLine("--> RabbitMq message bus publisher disposed");
    }
}