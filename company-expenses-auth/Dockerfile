# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Install Node.js for Tailwind CSS build
RUN apt-get update && apt-get install -y curl \
    && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs \
    && rm -rf /var/lib/apt/lists/*

# Copy project file
COPY company-expenses-auth/company-expenses-auth.csproj ./company-expenses-auth/
COPY company-expenses-auth/package.json ./company-expenses-auth/

# Restore dependencies
RUN dotnet restore company-expenses-auth/company-expenses-auth.csproj

# Copy source code
COPY company-expenses-auth/ ./company-expenses-auth/

# Build Tailwind CSS
WORKDIR /src/company-expenses-auth
RUN npm install && npm run build:css || true

# Build the application
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create shared-keys directory
RUN mkdir -p /app/shared-keys

# Copy published files
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80

ENTRYPOINT ["dotnet", "company-expenses-auth.dll"]
