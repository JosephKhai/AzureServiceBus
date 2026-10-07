using AzureServiceBus.Contracts;

namespace AzureServiceBus
{
    public interface IServiceBusPublisher
    {
        Task ServiceBusPublish(ProductCreatedEvent product, CancellationToken cancellationToken = default);
    }
}
