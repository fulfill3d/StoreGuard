namespace StoreGuard.Integrations.CosmosDbClient.Interfaces
{
    public interface ICosmosDbClient
    {
        Task AddItemAsync<T>(T item, string partitionKey);
        Task<T> GetItemAsync<T>(string id, string partitionKey);
        Task<IEnumerable<T>> QueryItemsAsync<T>(string query);
        Task ReplaceItemAsync<T>(string id, T item, string partitionKey);
        Task DeleteItemAsync(string id, string partitionKey);
    }
}