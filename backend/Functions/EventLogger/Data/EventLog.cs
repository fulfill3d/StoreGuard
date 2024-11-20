using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace StoreGuard.Functions.EventLogger.Data
{
    public class EventLog
    {
        [JsonProperty("id")] public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty("uuid")] public string UUID { get; set; }
        [JsonProperty("start_time")] public DateTime StartTime { get; set; }
        [JsonProperty("end_time")] public DateTime EndTime { get; set; }
        [JsonProperty("source_id")] public string SourceId { get; set; }
        [JsonProperty("camera_id")] public string CameraId { get; set; }
    }
}