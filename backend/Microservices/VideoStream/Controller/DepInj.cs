using StoreGuard.Common.Services;
using StoreGuard.Integrations.EventHubClient;
using StoreGuard.Integrations.EventHubClient.Options;
using StoreGuard.Microservices.VideoStream.Service;
using StoreGuard.Microservices.VideoStream.Service.Interfaces;

namespace StoreGuard.Microservices.VideoStream
{
    public static class DepInj
    {
        public static IServiceCollection RegisterServices(
            this IServiceCollection services, 
            Action<EventHubClientOptions> configureEventHub)
        {
            services.RegisterEventHubClient(configureEventHub);
            services.AddTransient<IVideoStreamService, VideoStreamService>();
            return services;
        }
    }
    
}