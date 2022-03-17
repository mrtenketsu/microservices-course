using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBus;

public class RabbitMqMessageBusListener : IMessageBusListener, IDisposable
{
    private readonly IServiceProvider serviceProvider;
    private readonly IMessageBusSubscriptionManager subscriptionManager;
    private readonly IRabbitMqMessageBusSettings settings;
    private readonly ILogger<RabbitMqMessageBusListener> logger;
    private readonly IConnection connection;
    private readonly IModel channel;

    public RabbitMqMessageBusListener(
        IServiceProvider serviceProvider,
        IMessageBusSubscriptionManager subscriptionManager,
        IRabbitMqMessageBusSettings settings,
        ILogger<RabbitMqMessageBusListener> logger)
    {
        this.serviceProvider = serviceProvider;
        this.subscriptionManager = subscriptionManager;
        this.settings = settings;
        this.logger = logger;

        logger.LogInformation("Attempting to connect {Host}:{Port}", settings.Host, settings.Port);
        
        var factory = new ConnectionFactory()
        {
            HostName = settings.Host,
            Port = settings.Port,
            DispatchConsumersAsync = true,
        };

        try
        {
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: settings.Exchange, type: settings.ExchangeType);
            channel.QueueDeclare(queue: settings.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            
            connection.ConnectionShutdown += OnConnectionShutdown;
            
            logger.LogInformation("Connected to RabbitMq message bus");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not connect to RabbitMq message bus: {Message}", ex.Message);
        }
    }

    private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        logger.LogWarning("Connection shutdown, cause: {Cause}", e.Cause);
    }

    public void Subscribe<TMessageHandler, TMessage>() where TMessageHandler : class, IMessageHandler<TMessage>
    {
        subscriptionManager.AddMessageHandler<TMessageHandler, TMessage>();
        var messageType = typeof(TMessage);
        var messageTypeName = messageType.Name;

        if (subscriptionManager.IsMessageTypeRegistered(messageTypeName))
        {
            logger.LogInformation("Subscribing to message {messageTypeName} with {Name}", messageTypeName, typeof(TMessageHandler).Name);
            
            channel.QueueBind(queue: settings.Queue,
                exchange: settings.Exchange,
                routingKey: messageTypeName);
        }
    }

    public void StartReceive(CancellationToken cancellationToken)
    {
        if (channel == null)
        {
            logger.LogWarning("Can't start receiving, consumer channel is not created");
            return;
        }
        StartBasicConsume();
    }

    public void Dispose()
    {
        logger.LogDebug("RabbitMq message bus listener disposing");

        if (channel?.IsOpen == true)
        {
            channel.Close();
            connection.Close();
        }
        
        channel?.Dispose();
        connection?.Dispose();
        
        logger.LogDebug("RabbitMq message bus listener disposed");
    }

    private void StartBasicConsume()
    {
        logger.LogInformation("Starting basic consume");

        if (channel != null)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += ConsumerOnReceived;
            
            channel.BasicConsume(
                queue: settings.Queue,
                autoAck: false,
                consumer: consumer);
        }
    }

    private async Task ConsumerOnReceived(object sender, BasicDeliverEventArgs args)
    {
        var messageTypeName = args.RoutingKey;
        var message = Encoding.UTF8.GetString(args.Body.Span);

        try
        {
            await ProcessMessage(message, messageTypeName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "--> Error processing message {message}: {Message}", message, ex.Message);
        }
        
        channel.BasicAck(args.DeliveryTag, multiple: false);
    }

    private async Task ProcessMessage(string messageJson, string messageTypeName)
    {
        if (!subscriptionManager.IsMessageTypeRegistered(messageTypeName))
            return;

        logger.LogDebug("Handling message {messageTypeName}: {messageJson}", messageTypeName, messageJson);
        var messageType = subscriptionManager.GetMessageType(messageTypeName);
        var message = JsonSerializer.Deserialize(messageJson, messageType);
        
        var handlerTypes = subscriptionManager.GetMessageHandlers(messageTypeName);
        
        for (int i = 0; i < handlerTypes.Count; i++)
        {
            using var scope = serviceProvider.CreateScope();
            var handlerType = handlerTypes[i];
            var hanlder = scope.ServiceProvider.GetService(handlerType);
            if (hanlder == null)
                continue;
            
            await (Task)handlerType.GetMethod("HandleMessage").Invoke(hanlder, new object[] { message });
        }
    }
}