using System.Text;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using StoreGuard.Integrations.EventHubClient.Interfaces;

namespace StoreGuard.Integrations.EventHubClient
{
    public class EventHubClient(EventHubProducerClient producerClient) : IEventHubClient
    {
        // Method to send a single message with metadata
        public async Task SendMessageAsync(string message, string? contentType = null, string? messageId = null, string? partitionKey = null)
        {
            var eventData = new EventData(Encoding.UTF8.GetBytes(message))
            {
                ContentType = contentType,
                MessageId = messageId
            };

            if (partitionKey != null)
            {
                // Send with partition key
                await producerClient.SendAsync(new[] { eventData }, new SendEventOptions { PartitionKey = partitionKey });
            }
            else
            {
                await producerClient.SendAsync(new[] { eventData });
            }
        }

        // Method to send a batch of messages with metadata
        public async Task SendBatchMessagesAsync(IEnumerable<string> messages, string? contentType = null, string? partitionKey = null)
        {
            var eventBatch = await producerClient.CreateBatchAsync(new CreateBatchOptions { PartitionKey = partitionKey });

            foreach (var message in messages)
            {
                var eventData = new EventData(Encoding.UTF8.GetBytes(message))
                {
                    ContentType = contentType,
                    MessageId = Guid.NewGuid().ToString()
                };

                if (!eventBatch.TryAdd(eventData))
                {
                    throw new Exception("Event data too large for the batch.");
                }
            }

            await producerClient.SendAsync(eventBatch);
        }

        // Method to send byte data with metadata
        public async Task SendByteDataAsync(byte[] data, string? contentType = null, string? messageId = null, string? partitionKey = null)
        {
            var eventData = new EventData(data)
            {
                ContentType = contentType,
                MessageId = messageId
            };

            if (partitionKey != null)
            {
                await producerClient.SendAsync(new[] { eventData }, new SendEventOptions { PartitionKey = partitionKey });
            }
            else
            {
                await producerClient.SendAsync(new[] { eventData });
            }
        }
    }
}
