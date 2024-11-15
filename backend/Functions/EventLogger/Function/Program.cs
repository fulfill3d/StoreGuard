using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using StoreGuard.Functions.EventLogger;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        var configuration = builder.Build();
        var token = new DefaultAzureCredential();
        var appConfigUrl = configuration["AppConfigUrl"] ?? string.Empty;

        builder.AddAzureAppConfiguration(config =>
        {
            config.Connect(new Uri(appConfigUrl), token);
            config.ConfigureKeyVault(kv => kv.SetCredential(token));
        });
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services.RegisterServices(cosmosOptions =>
        {
            cosmosOptions.EndpointUri = configuration["Fulfill3dCosmosEndpointUri"] ?? string.Empty;
            cosmosOptions.PrimaryKey = configuration["Fulfill3dCosmosPrimaryKey"] ?? string.Empty;
            cosmosOptions.DatabaseId = configuration["StoreGuardCosmosDatabaseId"] ?? string.Empty;
            cosmosOptions.ContainerId = configuration["StoreGuardCosmosContainerId"] ?? string.Empty;
        });
    })
    .Build();

host.Run();