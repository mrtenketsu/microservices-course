namespace MessageBus;

public interface IMessageBusSubscriptionManager
{
    void AddMessageHandler<TMessageHandler, TMessage>()
        where TMessageHandler : class, IMessageHandler<TMessage>;

    bool IsMessageTypeRegistered(string messageTypeName);
    
    Type GetMessageType(string messageTypeName);
    
    List<Type> GetMessageHandlers(string messageTypeName);
}