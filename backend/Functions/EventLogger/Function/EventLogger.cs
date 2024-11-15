using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StoreGuard.Functions.EventLogger.Data;
using StoreGuard.Functions.EventLogger.Service.Interface;

namespace StoreGuard.Functions.EventLogger
{
    public class EventLogger(IEventLogService eventLogService)
    {
        
        [Function("service-bus-event-logger")]
        public async Task Run(
            [ServiceBusTrigger("service-bus-event-logger", Connection = "ServiceBusConnectionString")] EventLog message,
            ILogger log)
        {
            log.LogInformation("Service Bus Event Logger processing started.");
            
            // Log the message content
            log.LogInformation(JsonConvert.SerializeObject(message));

            try
            {
                // Write the event to Cosmos DB
                await eventLogService.AddEventLogAsync(new EventLog
                {
                    EventType = message.EventType,
                    Payload = message.Payload,
                    Timestamp = message.Timestamp
                });

                log.LogInformation("Event logged successfully.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error processing event log: {ex.Message}");
                throw;
            }
        }
    }
}