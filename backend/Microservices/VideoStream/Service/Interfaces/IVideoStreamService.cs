namespace StoreGuard.Microservices.VideoStream.Service.Interfaces
{
    public interface IVideoStreamService
    {
        Task SendVideoDataAsync(byte[] data);
    }
}