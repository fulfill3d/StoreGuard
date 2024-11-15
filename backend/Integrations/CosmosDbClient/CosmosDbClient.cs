using Microsoft.Azure.Cosmos;
using StoreGuard.Integrations.CosmosDbClient.Interfaces;

namespace StoreGuard.Integrations.CosmosDbClient
{
    public class CosmosDbClient(Container container) : ICosmosDbClient
    {
        public async Task AddItemAsync<T>(T item, string partitionKey)
        {
            await container.CreateItemAsync(item, new PartitionKey(partitionKey));
        }

        public async Task<T> GetItemAsync<T>(string id, string partitionKey)
        {
            ItemResponse<T> response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
            return response.Resource;
        }

        public async Task<IEnumerable<T>> QueryItemsAsync<T>(string query)
        {
            var queryDefinition = new QueryDefinition(query);
            var queryResultSetIterator = container.GetItemQueryIterator<T>(queryDefinition);

            List<T> results = new List<T>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                results.AddRange(currentResultSet.Resource);
            }
            return results;
        }

        public async Task ReplaceItemAsync<T>(string id, T item, string partitionKey)
        {
            await container.ReplaceItemAsync(item, id, new PartitionKey(partitionKey));
        }

        public async Task DeleteItemAsync(string id, string partitionKey)
        {
            await container.DeleteItemAsync<object>(id, new PartitionKey(partitionKey));
        }
    }
}
