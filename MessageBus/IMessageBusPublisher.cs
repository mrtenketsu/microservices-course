namespace MessageBus;

public interface IMessageBusPublisher
{
    void Publish(object message);
}