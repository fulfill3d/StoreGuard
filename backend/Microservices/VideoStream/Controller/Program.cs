using Azure.Identity;
using StoreGuard.Microservices.VideoStream;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

var token = new DefaultAzureCredential();
var appConfigUrl = configuration["AppConfigUrl"] ?? string.Empty;

configuration.AddAzureAppConfiguration(config =>
{
    config.Connect(new Uri(appConfigUrl), token);
    config.ConfigureKeyVault(kv => kv.SetCredential(token));
});


services.RegisterServices(configureEventHub =>
{
    configureEventHub.ConnectionString = configuration["EventHubConnectionString"] ?? string.Empty;
    configureEventHub.EventHubName = configuration["EventHubName"] ?? string.Empty;
});

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");
app.UseStaticFiles();
app.MapHub<VideoSignalingHub>("/videoHub");
app.Run();
