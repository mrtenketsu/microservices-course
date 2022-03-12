namespace MessageBus;

public interface IMessageBusListener
{
    void Subscribe<TMessageHandler, TMessage>()
        where TMessageHandler : class, IMessageHandler<TMessage>;

    void StartReceive(CancellationToken cancellationToken);
}