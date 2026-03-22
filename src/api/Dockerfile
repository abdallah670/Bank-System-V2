# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY BankSystem.V2.sln .
COPY src/api/BankSystem.Api/BankSystem.Api.csproj src/api/BankSystem.Api/
COPY src/api/BankSystem.Core/BankSystem.Core.csproj src/api/BankSystem.Core/
COPY src/api/BankSystem.Infrastructure/BankSystem.Infrastructure.csproj src/api/BankSystem.Infrastructure/
COPY src/api/BankSystem.Services/BankSystem.Services.csproj src/api/BankSystem.Services/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY src/api/ src/api/

# Build
RUN dotnet build src/api/BankSystem.Api/BankSystem.Api.csproj -c Release -o /app/build

# Publish
RUN dotnet publish src/api/BankSystem.Api/BankSystem.Api.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 7000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:7000/api/health || exit 1

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:7000

ENTRYPOINT ["dotnet", "BankSystem.Api.dll"]
