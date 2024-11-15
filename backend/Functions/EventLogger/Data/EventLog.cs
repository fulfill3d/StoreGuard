namespace StoreGuard.Functions.EventLogger.Data
{
    public class EventLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; }
        public string Payload { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}