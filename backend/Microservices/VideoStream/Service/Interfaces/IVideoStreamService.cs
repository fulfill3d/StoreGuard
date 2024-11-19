namespace StoreGuard.Microservices.VideoStream.Service.Interfaces
{
    public interface IVideoStreamService
    {
        Task SendVideoDataAsync(string base64Data, string sourceId, string cameraId, string timestamp);
    }
}