using Azure.Identity;
using StoreGuard.Microservices.VideoStream;

var builder = WebApplication.CreateBuilder(args);

var token = new DefaultAzureCredential();
var appConfigUrl = builder.Configuration["AppConfigUrl"] ?? string.Empty;

builder.Configuration.AddAzureAppConfiguration(config =>
{
    config.Connect(new Uri(appConfigUrl), token);
    config.ConfigureKeyVault(kv => kv.SetCredential(token));
});

var services = builder.Services;

services.RegisterServices(configureEventHub =>
{
    configureEventHub.ConnectionString = builder.Configuration["EventHubConnectionString"] ?? string.Empty;
    configureEventHub.EventHubName = builder.Configuration["EventHubName"] ?? string.Empty;
});

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");
app.UseStaticFiles();
app.MapHub<VideoSignalingHub>("/videoHub");
app.Run();
