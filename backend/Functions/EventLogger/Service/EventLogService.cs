using StoreGuard.Functions.EventLogger.Data;
using StoreGuard.Functions.EventLogger.Service.Interface;
using StoreGuard.Integrations.CosmosDbClient.Interfaces;

namespace StoreGuard.Functions.EventLogger.Service
{
    public class EventLogService(ICosmosDbClient cosmosDbClient) : IEventLogService
    {

        public async Task AddEventLogAsync(EventLog eventLog)
        {
            await cosmosDbClient.AddItemAsync(eventLog, eventLog.SourceId);
        }
    }
}