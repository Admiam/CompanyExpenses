# Docker Deployment Guide

This guide explains how to deploy Company Expenses using Docker Compose with a single command.

## Prerequisites

- **Docker Desktop** installed ([Download Docker](https://www.docker.com/products/docker-desktop/))
  - Windows: Docker Desktop for Windows
  - macOS: Docker Desktop for Mac
  - Linux: Docker Engine + Docker Compose

No other dependencies required! Docker handles everything:

- .NET 10 runtime
- Node.js
- SQL Server
- All npm packages

## Quick Start

### Option 1: Using Start Script (Recommended)

**macOS/Linux:**

```bash
chmod +x start.sh
./start.sh
```

**Windows (Command Prompt):**

```cmd
start.bat
```

**Windows (PowerShell):**

```powershell
.\start.ps1
```

### Option 2: Using Docker Compose Directly

```bash
docker compose up --build -d
```

## Default Admin Credentials

After the application starts, you can log in with:

| Field    | Value                          |
| -------- | ------------------------------ |
| Email    | `admin@company-expenses.local` |
| Password | `Admin123!`                    |

## Service URLs

Once running, the services are available at:

| Service      | URL                   |
| ------------ | --------------------- |
| Frontend App | http://localhost:3000 |
| Auth Server  | http://localhost:5169 |
| API Server   | http://localhost:5200 |
| SQL Server   | localhost:1433        |

## Useful Commands

### View logs

```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f api
docker compose logs -f auth
docker compose logs -f app
docker compose logs -f db
```

### Stop all services

```bash
docker compose down
```

### Stop and remove all data (clean start)

```bash
docker compose down -v
```

### Rebuild a specific service

```bash
docker compose build api
docker compose up -d api
```

### Check service status

```bash
docker compose ps
```

### Access database

```bash
docker compose exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "CompanyExpenses123!" -C
```

## Configuration

### Environment Variables

You can customize the deployment by setting environment variables before running:

```bash
# Optional: Google OAuth (for social login)
export GOOGLE_CLIENT_ID="your-google-client-id"
export GOOGLE_CLIENT_SECRET="your-google-client-secret"

# Then run
docker compose up --build -d
```

### Custom Ports

Edit `docker-compose.yml` to change ports:

```yaml
services:
  app:
    ports:
      - "8080:80" # Change 8080 to your preferred port
```

## Troubleshooting

### Database connection issues

```bash
# Check if database is running
docker compose ps db

# View database logs
docker compose logs db

# Restart database
docker compose restart db
```

### Application won't start

```bash
# Check logs for errors
docker compose logs api
docker compose logs auth

# Rebuild from scratch
docker compose down -v
docker compose up --build -d
```

### Clear everything and start fresh

```bash
# Remove all containers, volumes, and images
docker compose down -v --rmi all

# Start fresh
docker compose up --build -d
```

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Docker Network                        │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐     │
│  │   Frontend  │    │    Auth     │    │     API     │     │
│  │    (React)  │───▶│   Server    │───▶│   Server    │     │
│  │  Port 3000  │    │  Port 5169  │    │  Port 5200  │     │
│  └─────────────┘    └──────┬──────┘    └──────┬──────┘     │
│                            │                   │            │
│                            ▼                   ▼            │
│                     ┌─────────────────────────────┐        │
│                     │        SQL Server           │        │
│                     │         Port 1433           │        │
│                     │  ┌─────────┐ ┌───────────┐  │        │
│                     │  │  Auth   │ │   Main    │  │        │
│                     │  │   DB    │ │    DB     │  │        │
│                     │  └─────────┘ └───────────┘  │        │
│                     └─────────────────────────────┘        │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Data Persistence

Data is persisted in Docker volumes:

- `sqlserver_data` - Database files
- `shared_keys` - Authentication keys (shared between auth and api)
- `uploads_data` - Uploaded expense attachments

To backup:

```bash
docker run --rm -v companyexpenses_sqlserver_data:/data -v $(pwd):/backup alpine tar czf /backup/db-backup.tar.gz /data
```

## Production Considerations

For production deployment:

1. **Change default passwords** in `docker-compose.yml`
2. **Configure HTTPS** using a reverse proxy (nginx, traefik)
3. **Set up proper email** for password resets
4. **Configure Google OAuth** for social login
5. **Use external database** for better performance and backup options
