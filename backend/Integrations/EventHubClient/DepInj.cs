using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StoreGuard.Common.Services;
using StoreGuard.Integrations.EventHubClient.Interfaces;
using StoreGuard.Integrations.EventHubClient.Options;

namespace StoreGuard.Integrations.EventHubClient
{
    public static class DepInj
    {
        public static void RegisterEventHubClient(
            this IServiceCollection services,
            Action<EventHubClientOptions> configureEventHubOptions)
        {
            services.ConfigureServiceOptions<EventHubClientOptions>((_, options) => configureEventHubOptions(options));

            services.AddSingleton<EventHubProducerClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<EventHubClientOptions>>().Value;
                return new EventHubProducerClient(options.ConnectionString, options.EventHubName);
            });

            services.AddTransient<IEventHubClient, EventHubClient>();
        }
    }
}