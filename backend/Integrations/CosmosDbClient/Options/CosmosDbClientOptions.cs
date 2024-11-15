namespace StoreGuard.Integrations.CosmosDbClient.Options
{
    public class CosmosDbClientOptions
    {
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
        public string DatabaseId { get; set; }
        public string ContainerId { get; set; }
    }
}