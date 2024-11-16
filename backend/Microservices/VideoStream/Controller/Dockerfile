# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY StoreGuard.Microservices.VideoStream.csproj ./
RUN dotnet restore StoreGuard.Microservices.VideoStream.csproj

# Copy the entire project and build it
COPY . .
RUN dotnet publish StoreGuard.Microservices.VideoStream.csproj -c Release -o /app/publish

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port the application runs on
EXPOSE 80

# Set the entry point to the built application
ENTRYPOINT ["dotnet", "StoreGuard.Microservices.VideoStream.dll"]
