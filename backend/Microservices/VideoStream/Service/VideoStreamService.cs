using System.Text;
using System.Text.Json;
using StoreGuard.Integrations.EventHubClient.Interfaces;
using StoreGuard.Microservices.VideoStream.Data.Models;
using StoreGuard.Microservices.VideoStream.Service.Interfaces;

namespace StoreGuard.Microservices.VideoStream.Service
{
    public class VideoStreamService(IEventHubClient eventHubClient): IVideoStreamService
    {
        public async Task SendVideoDataAsync(byte[] data, string sourceId, string cameraId)
        {
            var payload = new Chunk
            {
                SourceId = sourceId,
                CameraId = cameraId,
                FrameData = Convert.ToBase64String(data)
            };
            
            var payloadJson = JsonSerializer.Serialize(payload);
            var bytePayload = Encoding.UTF8.GetBytes(payloadJson);
            
            await eventHubClient.SendByteDataAsync(bytePayload, null, cameraId, sourceId);
        }
    }
}