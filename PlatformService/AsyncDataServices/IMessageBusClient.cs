using PlatformService.Dtos;

namespace PlatformService.AsyncDataServices;

public interface IMessageBusClient
{
    void Publish(object message);
}