using Azure.Messaging.ServiceBus;
using AzureServiceBusConsumer;
using AzureServiceBusConsumer.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddOptions<ServiceBusOptions>()
    .Bind(builder.Configuration.GetSection(ServiceBusOptions.SectionName))
    .PostConfigure(options =>
    {
        options.ConnectionString = options.ConnectionString?.Trim() ?? string.Empty;
        options.QueueName = options.QueueName?.Trim() ?? string.Empty;
        options.TopicName = options.TopicName?.Trim() ?? string.Empty;
        options.SubscriptionName = options.SubscriptionName?.Trim() ?? string.Empty;
    })
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "ServiceBus:ConnectionString is not configured.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.QueueName), "ServiceBus:QueueName is not configured.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.TopicName), "ServiceBus:TopicName is not configured.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.SubscriptionName), "ServiceBus:SubscriptionName is not configured.")
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
{
    var serviceBusOptions = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
    return new ServiceBusClient(serviceBusOptions.ConnectionString);
});
builder.Services.AddHostedService<ProductCreatedConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
