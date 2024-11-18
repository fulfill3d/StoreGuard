using StoreGuard.Integrations.EventHubClient;
using StoreGuard.Integrations.EventHubClient.Options;
using StoreGuard.Microservices.VideoStream.Service;
using StoreGuard.Microservices.VideoStream.Service.Interfaces;

namespace StoreGuard.Microservices.VideoStream
{
    public static class DepInj
    {
        public static void RegisterServices(
            this IServiceCollection services, 
            Action<EventHubClientOptions> configureEventHub)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    builder.WithOrigins("http://localhost:63342", "http://localhost:5026")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            services.RegisterEventHubClient(configureEventHub);
            services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 1024 * 1024 * 10;
            });
            services.AddSingleton<IVideoStreamService, VideoStreamService>();
        }
    }
    
}