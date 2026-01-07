# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY CompanyExpenses.sln ./
COPY company-expenses-api/company-expenses-api.csproj ./company-expenses-api/
COPY company-expenses-auth/company-expenses-auth.csproj ./company-expenses-auth/
COPY company-expenses-database/company-expenses-database.csproj ./company-expenses-database/
COPY company-expenses-models/company-expenses-models.csproj ./company-expenses-models/
COPY company-expenses-services/company-expenses-services.csproj ./company-expenses-services/

# Restore dependencies
RUN dotnet restore company-expenses-api/company-expenses-api.csproj

# Copy all source code
COPY company-expenses-api/ ./company-expenses-api/
COPY company-expenses-database/ ./company-expenses-database/
COPY company-expenses-models/ ./company-expenses-models/
COPY company-expenses-services/ ./company-expenses-services/

# Build the application
WORKDIR /src/company-expenses-api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create directories
RUN mkdir -p /app/uploads/receipts /app/shared-keys

# Copy published files
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80

ENTRYPOINT ["dotnet", "company-expenses-api.dll"]
