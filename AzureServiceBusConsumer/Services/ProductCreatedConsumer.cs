using System.Text.Json;
using Azure.Messaging.ServiceBus;
using AzureServiceBusConsumer.Contracts;
using Microsoft.Extensions.Options;

namespace AzureServiceBusConsumer.Services;

public sealed class ProductCreatedConsumer(
    ServiceBusClient client,
    IOptions<ServiceBusOptions> options,
    ILogger<ProductCreatedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // With Topic multiple Subscription Method
        var topicName = options.Value.TopicName;
        var subscriptionName = options.Value.SubscriptionName;


        await using var processor = client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

        processor.ProcessMessageAsync += args => HandleMessageAsync(args, "topic-subscription");
        processor.ProcessErrorAsync += HandleErrorAsync;


        // with One to One Queue
        //var queueName = options.Value.QueueName;

        //await using var processor = client.CreateProcessor(queueName,  new ServiceBusProcessorOptions
        //{
        //    AutoCompleteMessages = false,
        //    MaxConcurrentCalls = 1
        //});

        //processor.ProcessMessageAsync += args => HandleMessageAsync(args, "queue");
        //processor.ProcessErrorAsync += HandleErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);

        //logger.LogInformation("Listening for ProductCreatedEvent messages on queue {QueueName}", queueName);
        logger.LogInformation("Listening for ProductCreatedEvent messages on topic {TopicName} and subscription {SubscriptionName}", topicName, subscriptionName);


        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            await processor.StopProcessingAsync(CancellationToken.None);
        }
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args, string source)
    {
        try
        {
            var messageBody = args.Message.Body.ToString();
            logger.LogInformation("Received message from {Source}: {MessageBody}", source, messageBody);

            var productCreated = JsonSerializer.Deserialize<ProductCreatedEvent>(messageBody);

            if (productCreated != null)
            {
                logger.LogInformation(
                "Received product {ProductId}: {ProductName} (event {EventId})",
                productCreated.ProductId,
                productCreated.Name,
                productCreated.EventId);

                Console.WriteLine(productCreated);

                await args.CompleteMessageAsync(args.Message);
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message: {MessageId}", args.Message.MessageId);
            await args.AbandonMessageAsync(args.Message);
            return;
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(
            args.Exception,
            "Service Bus error from {ErrorSource} for {EntityPath}",
            args.ErrorSource,
            args.EntityPath);

        return Task.CompletedTask;
    }
}
