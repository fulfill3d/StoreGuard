using Microsoft.Extensions.DependencyInjection;
using StoreGuard.Integrations.CosmosDbClient.Options;

namespace StoreGuard.Functions.EventLogger
{
    public static class DepInj
    {
        public static void RegisterServices(
            this IServiceCollection services,
            Action<CosmosDbClientOptions> configureCosmosDbClientOptions)
        {
        }
    }
}