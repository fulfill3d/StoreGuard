namespace StoreGuard.Integrations.EventHubClient.Interfaces
{
    public interface IEventHubClient
    {
        Task SendMessageAsync(string message);
        Task SendBatchMessagesAsync(IEnumerable<string> messages);
    }
}