using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBus;

public class RabbitMqMessageBusPublisher : IMessageBusPublisher, IDisposable
{
    private readonly IRabbitMqMessageBusSettings settings;
    private readonly ILogger<RabbitMqMessageBusPublisher> logger;
    private readonly IConnection connection;
    private readonly IModel channel;

    public RabbitMqMessageBusPublisher(IRabbitMqMessageBusSettings settings, ILogger<RabbitMqMessageBusPublisher> logger)
    {
        this.settings = settings;
        this.logger = logger;

        logger.LogInformation("RabbitMq message bus client, attempting to connect {Host}:{Port}", settings.Host, settings.Port);
        
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
            
            logger.LogInformation("Connected to RabbitMq message bus");
        }
        catch (Exception ex)
        {
            logger.LogError("Could not connect to RabbitMq message bus: {Message}", ex.Message);
        }
    }
    
    public void Publish(object message)
    {
        var messageType = message.GetType().Name;
        var messageJson = JsonSerializer.Serialize(message);
        
        if (connection.IsOpen)
        {
            logger.LogDebug("RabbitMq message bus connection is open, sending message of type {messageType}...", messageType);
            PublishInternal(messageJson, messageType);
        }
        else
        {
            logger.LogWarning("RabbitMq message bus connection is closed, not sending message of type {messageType}", messageType);
        }
    }
    
    private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        logger.LogWarning("RabbitMq message bus connection shutdown, cause: {Cause}", e.Cause);
    }

    private void PublishInternal(string messageJson, string messageType)
    {
        var body = Encoding.UTF8.GetBytes(messageJson);

        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        
        channel.BasicPublish(settings.Exchange, messageType, props, body);
        
        logger.LogDebug("RabbitMq message bus message of type {messageType} sent", messageType);
    }

    public void Dispose()
    {
        logger.LogDebug("RabbitMq message bus publisher disposing");

        if (channel?.IsOpen == true)
        {
            channel.Close();
            connection.Close();
        }
        
        channel?.Dispose();
        connection?.Dispose();
        
        logger.LogDebug("RabbitMq message bus publisher disposed");
    }
}