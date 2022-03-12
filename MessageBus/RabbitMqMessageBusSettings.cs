namespace MessageBus;

public class RabbitMqMessageBusSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Exchange { get; set; }
    public string ExchangeType { get; set; }
    public string Queue { get; set; }
    
}