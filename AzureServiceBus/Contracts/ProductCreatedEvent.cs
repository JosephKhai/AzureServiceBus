namespace AzureServiceBus.Contracts;

public sealed record ProductCreatedEvent(
    Guid EventId,
    int ProductId,
    string Name);
