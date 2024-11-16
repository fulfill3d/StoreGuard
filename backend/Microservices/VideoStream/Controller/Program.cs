using Azure.Messaging.EventHubs.Producer;
using StoreGuard.Microservices.VideoStream;

var builder = WebApplication.CreateBuilder(args);

// Step 1: Connect to Azure App Configuration
var appConfigConnectionString = builder.Configuration["AppConfig:ConnectionString"] ?? string.Empty;

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(appConfigConnectionString)
        .Select("*")  // Load all configuration settings
        .ConfigureRefresh(refresh =>
        {
            // Optional: Configure auto-refresh for specific keys
            refresh.Register("EventHub:ConnectionString", refreshAll: true);
        });
});

// Step 2: Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.RegisterServices();

// Step 3: Configure Azure Event Hubs using settings from App Configuration
var eventHubConnectionString = builder.Configuration["EventHub:ConnectionString"];
var eventHubName = builder.Configuration["EventHub:Name"];

builder.Services.AddSingleton(_ => new EventHubProducerClient(eventHubConnectionString, eventHubName));

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