using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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