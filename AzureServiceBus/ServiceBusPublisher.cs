using Azure.Messaging.ServiceBus;
using AzureServiceBus.Contracts;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AzureServiceBus
{
    public class ServiceBusPublisher : IServiceBusPublisher
    {
        private readonly string _queueName;
        private readonly string _topicName;
        private readonly ServiceBusSender _queueSender;
        private readonly ServiceBusSender _topicSender;

        public ServiceBusPublisher(IOptions<ServiceBusOptions> options, ServiceBusClient client)
        {
            _queueName = options.Value.QueueName;
            _topicName = options.Value.TopicName;

            _queueSender = client.CreateSender(_queueName);
            _topicSender = client.CreateSender(_topicName);
        }

        public async Task ServiceBusPublish(ProductCreatedEvent product, CancellationToken cancellationToken = default)
        {
            var messageBody = JsonSerializer.Serialize(product);

            var queueMessage = CreateMessage(product, messageBody);
            await _queueSender.SendMessageAsync(queueMessage, cancellationToken);

            var topicMessage = CreateMessage(product, messageBody);
            await _topicSender.SendMessageAsync(topicMessage, cancellationToken);
        }

        private static ServiceBusMessage CreateMessage(ProductCreatedEvent product, string messageBody)
        {
            return new ServiceBusMessage(messageBody)
            {
                MessageId = product.EventId.ToString(),
                Subject = product.ProductId.ToString(),
                ApplicationProperties =
                {
                    { "ProductId", product.ProductId },
                    { "Name", product.Name }
                }
            };
        }
    }
}   
