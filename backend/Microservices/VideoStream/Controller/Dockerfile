# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file
COPY StoreGuard.sln .

# Copy project files for dependency restoration
COPY Common/Services/StoreGuard.Common.Services.csproj Common/Services/
COPY Integrations/EventHubClient/StoreGuard.Integrations.EventHubClient.csproj Integrations/EventHubClient/
COPY Microservices/VideoStream/Data/StoreGuard.Microservices.VideoStream.Data.csproj Microservices/VideoStream/Data/
COPY Microservices/VideoStream/Service/StoreGuard.Microservices.VideoStream.Service.csproj Microservices/VideoStream/Service/
COPY Microservices/VideoStream/Controller/StoreGuard.Microservices.VideoStream.csproj Microservices/VideoStream/Controller/

# Restore dependencies for the VideoStream microservice
RUN dotnet restore Microservices/VideoStream/Controller/StoreGuard.Microservices.VideoStream.csproj

# Copy the rest of the source code
COPY Common/Services/* Common/Services/
COPY Integrations/EventHubClient/* Integrations/EventHubClient/
COPY Microservices/VideoStream/Data/* Microservices/VideoStream/Data/
COPY Microservices/VideoStream/Service/* Microservices/VideoStream/Service/
COPY Microservices/VideoStream/Controller/* Microservices/VideoStream/Controller/

# Build and publish the project
WORKDIR /src/Microservices/VideoStream/Controller
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port the application runs on
EXPOSE 80

# Set the entry point to the built application
ENTRYPOINT ["dotnet", "StoreGuard.Microservices.VideoStream.dll"]
