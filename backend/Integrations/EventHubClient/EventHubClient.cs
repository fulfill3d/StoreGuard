using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using StoreGuard.Integrations.EventHubClient.Interfaces;

namespace StoreGuard.Integrations.EventHubClient
{
    public class EventHubClient(EventHubProducerClient producerClient) : IEventHubClient
    {
        public async Task SendMessageAsync(string message)
        {
            using var eventBatch = await producerClient.CreateBatchAsync();
            eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(message)));

            await producerClient.SendAsync(eventBatch);
        }

        public async Task SendBatchMessagesAsync(IEnumerable<string> messages)
        {
            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

            foreach (var message in messages)
            {
                if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(message))))
                {
                    throw new Exception("Event data too large for the batch.");
                }
            }

            await producerClient.SendAsync(eventBatch);
        }
    }
}