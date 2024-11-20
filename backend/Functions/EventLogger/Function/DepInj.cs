using Microsoft.Extensions.DependencyInjection;
using StoreGuard.Functions.EventLogger.Service;
using StoreGuard.Functions.EventLogger.Service.Interface;
using StoreGuard.Integrations.CosmosDbClient;
using StoreGuard.Integrations.CosmosDbClient.Options;

namespace StoreGuard.Functions.EventLogger
{
    public static class DepInj
    {
        public static void RegisterServices(
            this IServiceCollection services,
            Action<CosmosDbClientOptions> cosmosDbClientOptions)
        {
            services.RegisterCosmosDbClient(cosmosDbClientOptions);
            services.AddTransient<IEventLogService, EventLogService>();
        }
    }
}