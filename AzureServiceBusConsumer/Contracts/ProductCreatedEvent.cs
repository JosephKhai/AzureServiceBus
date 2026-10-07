namespace AzureServiceBusConsumer.Contracts;

public sealed record ProductCreatedEvent(
    Guid EventId,
    int ProductId,
    string Name);
