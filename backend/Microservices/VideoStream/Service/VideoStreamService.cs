using StoreGuard.Integrations.EventHubClient.Interfaces;
using StoreGuard.Microservices.VideoStream.Service.Interfaces;

namespace StoreGuard.Microservices.VideoStream.Service
{
    public class VideoStreamService(IEventHubClient eventHubClient): IVideoStreamService
    {
        public async Task Test()
        {
            await eventHubClient.SendMessageAsync("test");
        }
    }
}