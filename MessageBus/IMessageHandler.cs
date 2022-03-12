namespace MessageBus;

public interface IMessageHandler<in TMessage>
{
    Task HandleMessage(TMessage dto);
}