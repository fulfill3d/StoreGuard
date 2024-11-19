using System.Text;
using System.Text.Json;
using StoreGuard.Integrations.EventHubClient.Interfaces;
using StoreGuard.Microservices.VideoStream.Data.Models;
using StoreGuard.Microservices.VideoStream.Service.Interfaces;

namespace StoreGuard.Microservices.VideoStream.Service
{
    public class VideoStreamService(IEventHubClient eventHubClient): IVideoStreamService
    {
        public async Task SendVideoDataAsync(string base64Data, string sourceId, string cameraId, string timestamp)
        {
            var payload = new Chunk
            {
                SourceId = sourceId,
                CameraId = cameraId,
                TimeStamp = timestamp,
                FrameData = base64Data
            };
            
            var payloadJson = JsonSerializer.Serialize(payload);
            var bytePayload = Encoding.UTF8.GetBytes(payloadJson);
            
            await eventHubClient.SendByteDataAsync(bytePayload, null, cameraId, sourceId);
        }
    }
}