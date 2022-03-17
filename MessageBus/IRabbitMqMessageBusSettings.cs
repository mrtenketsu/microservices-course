namespace MessageBus;

public interface IRabbitMqMessageBusSettings
{
    string Host { get; }
    int Port { get; }
    string Exchange { get; }
    string ExchangeType { get; }
    string Queue { get; }
}