using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StoreGuard.Functions.EventLogger.Data;
using StoreGuard.Functions.EventLogger.Service.Interface;

namespace StoreGuard.Functions.EventLogger
{
    public class EventLogger(IEventLogService eventLogService, ILogger<EventLogger> logger)
    {
        
        [Function("sg-video-service")]
        public async Task Run([ServiceBusTrigger("sg-video-service", Connection = "ServiceBusConnectionString", IsSessionsEnabled = true)]  ServiceBusReceivedMessage msg)
        {
            var message = JsonConvert.DeserializeObject<EventLog>(Encoding.UTF8.GetString(msg.Body));

            if (message != null)
            {
                await eventLogService.AddEventLogAsync(message);
            }
            else
            {
                logger.LogError("EventLogger: Failed to deserialize message");
            }
        }
    }
}