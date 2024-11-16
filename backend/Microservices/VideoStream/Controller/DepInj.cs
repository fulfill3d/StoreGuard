using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StoreGuard.Microservices.VideoStream
{
    public static class DepInj
    {
        /// <summary>
        /// Adds Azure App Configuration and Event Hub dependencies to the IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="configuration">The configuration object to retrieve settings from.</param>
        /// <returns>The IServiceCollection with the added services.</returns>
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {

            // services.AddSingleton(_ => new EventHubProducerClient(eventHubConnectionString, eventHubName));

            return services;
        }
    }
    
}