using AzureServiceBus.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AzureServiceBus.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly IServiceBusPublisher _serviceBusPublisher;

    public ProductsController(IServiceBusPublisher serviceBusPublisher)
    {
        _serviceBusPublisher = serviceBusPublisher;
    }

    [HttpPost]
    public async Task<ActionResult<ProductCreatedEvent>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var productCreated = new ProductCreatedEvent(
            Guid.NewGuid(),
            request.Id,
            request.Name);

        await _serviceBusPublisher.ServiceBusPublish(productCreated, cancellationToken);


        return Accepted(productCreated);
    }
}

public sealed record CreateProductRequest(int Id, string Name);
