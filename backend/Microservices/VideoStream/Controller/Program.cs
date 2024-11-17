using Azure.Identity;
using StoreGuard.Microservices.VideoStream;

var builder = WebApplication.CreateBuilder(args);

// Connect to Azure App Configuration
var token = new DefaultAzureCredential();
var appConfigUrl = builder.Configuration["AppConfigUrl"] ?? string.Empty;

builder.Configuration.AddAzureAppConfiguration(config =>
{
    config.Connect(new Uri(appConfigUrl), token);
    config.ConfigureKeyVault(kv => kv.SetCredential(token));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container
builder.Services.RegisterServices(configureEventHub =>
{
    configureEventHub.ConnectionString = builder.Configuration["EventHubConnectionString"] ?? string.Empty;
    configureEventHub.EventHubName = builder.Configuration["EventHubName"] ?? string.Empty;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map the controllers
app.MapControllers();

app.Run();