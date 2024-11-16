using System.Text.Json.Serialization;

namespace StoreGuard.Microservices.VideoStream.Data.Models
{
    public class Node
    {
        // a Node is a meta data that represents the IoT streaming video content
        [JsonPropertyName("uuid")] public string UUID { get; set; }
        [JsonPropertyName("data")] public byte[] Data { get; set; }
    }
}