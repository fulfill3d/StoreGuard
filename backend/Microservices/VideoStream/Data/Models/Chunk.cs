namespace StoreGuard.Microservices.VideoStream.Data.Models
{
    public class Chunk
    {
        public string SourceId { get; set; }
        public string CameraId { get; set; }
        public string FrameData { get; set; }
    }
}