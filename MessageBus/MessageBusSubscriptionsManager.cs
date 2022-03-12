namespace MessageBus;

public class MessageBusSubscriptionsManager : IMessageBusSubscriptionManager
{
    private readonly Dictionary<string, List<Type>> handlers = new();
    private readonly Dictionary<string, Type> messageTypes = new();

    public void AddMessageHandler<TMessageHandler, TMessage>() where TMessageHandler : class, IMessageHandler<TMessage>
    {
        var messageType = typeof(TMessage);
        var messageTypeName = messageType.Name;
        var handlerType = typeof(TMessageHandler);

        if (!messageTypes.ContainsKey(messageTypeName))
            messageTypes.Add(messageTypeName, messageType);

        if (handlers.TryGetValue(messageTypeName, out var handlersList))
        {
            if (!handlersList.Contains((handlerType)))
                handlersList.Add(handlerType);
        }
        else
        {
            handlersList = new List<Type> {handlerType};
            handlers.Add(messageTypeName, handlersList);
        }
    }

    public bool IsMessageTypeRegistered(string messageTypeName)
    {
        return messageTypes.ContainsKey(messageTypeName);
    }

    public Type GetMessageType(string messageTypeName)
    {
        return messageTypes[messageTypeName];
    }

    public List<Type> GetMessageHandlers(string messageTypeName)
    {
        if (handlers.TryGetValue(messageTypeName, out var typeHandlers))
            return typeHandlers;
        
        return new List<Type>(0);
    }
    
}