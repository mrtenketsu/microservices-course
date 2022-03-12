using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBus;

public class RabbitMqMessageBusListener : IMessageBusListener, IDisposable
{
    private readonly IServiceProvider serviceProvider;
    private readonly IMessageBusSubscriptionManager subscriptionManager;
    private readonly RabbitMqMessageBusSettings settings;
    private readonly IConnection connection;
    private readonly IModel channel;

    public RabbitMqMessageBusListener(IServiceProvider serviceProvider, IMessageBusSubscriptionManager subscriptionManager, RabbitMqMessageBusSettings settings)
    {
        this.serviceProvider = serviceProvider;
        this.subscriptionManager = subscriptionManager;
        this.settings = settings;

        Console.WriteLine($"--> Attempting to connect {settings.Host}:{settings.Port}");
        
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
            
            Console.WriteLine($"--> Connected to RabbitMq message bus");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not connect to RabbitMq message bus: {ex.Message}");
        }
    }

    private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine($"--> Connection shutdown, cause: {e.Cause}");
    }

    public void Subscribe<TMessageHandler, TMessage>() where TMessageHandler : class, IMessageHandler<TMessage>
    {
        subscriptionManager.AddMessageHandler<TMessageHandler, TMessage>();
        var messageType = typeof(TMessage);
        var messageTypeName = messageType.Name;

        if (subscriptionManager.IsMessageTypeRegistered(messageTypeName))
        {
            Console.WriteLine("--> Subscribing to message {0} with {1}", messageTypeName, typeof(TMessageHandler).Name);
            
            channel.QueueBind(queue: settings.Queue,
                exchange: settings.Exchange,
                routingKey: messageTypeName);
        }
    }

    public void StartReceive(CancellationToken cancellationToken)
    {
        if (channel == null)
        {
            Console.WriteLine("--> Can't start receiving, consumer channel is not created");
            return;
        }
        StartBasicConsume();
    }

    public void Dispose()
    {
        Console.WriteLine("--> RabbitMq message bus listener disposing");

        if (channel?.IsOpen == true)
        {
            channel.Close();
            connection.Close();
        }
        
        channel?.Dispose();
        connection?.Dispose();
        
        Console.WriteLine("--> RabbitMq message bus listener disposed");
    }

    private void StartBasicConsume()
    {
        Console.WriteLine("--> Starting basic consume");

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
            Console.WriteLine("--> Error processing message {0}: {1}", message, ex.Message);
        }
        
        channel.BasicAck(args.DeliveryTag, multiple: false);
    }

    private async Task ProcessMessage(string messageJson, string messageTypeName)
    {
        if (!subscriptionManager.IsMessageTypeRegistered(messageTypeName))
            return;

        Console.WriteLine("--> Handling message {0}: {1}", messageTypeName, messageJson);
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