namespace StoreGuard.Integrations.EventHubClient.Interfaces
{
    public interface IEventHubClient
    {
        // Method to send a single message with optional metadata
        Task SendMessageAsync(string message, string? contentType = null, string? messageId = null, string? partitionKey = null);

        // Method to send a batch of messages with optional metadata
        Task SendBatchMessagesAsync(IEnumerable<string> messages, string? contentType = null, string? partitionKey = null);

        // Method to send byte data with optional metadata
        Task SendByteDataAsync(byte[] data, string? contentType = null, string? messageId = null, string? partitionKey = null);
    }
}