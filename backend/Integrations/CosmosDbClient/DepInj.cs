using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StoreGuard.Integrations.CosmosDbClient.Interfaces;
using StoreGuard.Integrations.CosmosDbClient.Options;

namespace StoreGuard.Integrations.CosmosDbClient
{
    public static class DepInj
    {
        public static void RegisterCosmosDbClient(
            this IServiceCollection services,
            Action<CosmosDbClientOptions> configureCosmosDbClientOptions)
        {
            services.ConfigureServiceOptions<CosmosDbClientOptions>((_, options) => configureCosmosDbClientOptions(options));

            services.AddSingleton<CosmosClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<CosmosDbClientOptions>>().Value;
                return new CosmosClient(options.EndpointUri, options.PrimaryKey);
            });

            services.AddSingleton<Container>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<CosmosDbClientOptions>>().Value;
                var cosmosClient = sp.GetRequiredService<CosmosClient>();
                return cosmosClient.GetContainer(options.DatabaseId, options.ContainerId);
            });
            
            services.AddTransient<ICosmosDbClient, CosmosDbClient>();
        }

        private static void ConfigureServiceOptions<TOptions>(
            this IServiceCollection services,
            Action<IServiceProvider, TOptions> configure)
            where TOptions : class
        {
            services
                .AddOptions<TOptions>()
                .Configure<IServiceProvider>((options, resolver) => configure(resolver, options));
        }
    }
}