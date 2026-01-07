# PowerShell startup script for Company Expenses
# =====================================================
# Company Expenses - Docker Startup Script for Windows
# This script starts all services with a single command
# =====================================================

Write-Host ""
Write-Host "=============================================="
Write-Host "  Company Expenses - Docker Setup"
Write-Host "=============================================="
Write-Host ""

# Check if Docker is installed
try {
    docker --version | Out-Null
} catch {
    Write-Host "Error: Docker is not installed." -ForegroundColor Red
    Write-Host "Please install Docker from https://docker.com"
    exit 1
}

# Check if Docker Compose is available
try {
    docker compose version | Out-Null
} catch {
    Write-Host "Error: Docker Compose is not available." -ForegroundColor Red
    Write-Host "Please install Docker Compose or update Docker."
    exit 1
}

Write-Host "Starting all services..."
Write-Host ""

# Build and start all containers
docker compose up --build -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to start containers." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=============================================="
Write-Host "  Waiting for services to be ready..."
Write-Host "=============================================="
Write-Host ""

Write-Host "Waiting for database to be ready..."
Start-Sleep -Seconds 30

Write-Host "Waiting for applications to initialize..."
Start-Sleep -Seconds 20

Write-Host ""
Write-Host "=============================================="
Write-Host "  Company Expenses is now running!" -ForegroundColor Green
Write-Host "=============================================="
Write-Host ""
Write-Host "  Frontend App:   " -NoNewline; Write-Host "http://localhost:3000" -ForegroundColor Cyan
Write-Host "  Auth Server:    " -NoNewline; Write-Host "http://localhost:5169" -ForegroundColor Cyan
Write-Host "  API Server:     " -NoNewline; Write-Host "http://localhost:5200" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Admin Login Credentials:"
Write-Host "    Email:    " -NoNewline; Write-Host "admin@company-expenses.local" -ForegroundColor Yellow
Write-Host "    Password: " -NoNewline; Write-Host "Admin123!" -ForegroundColor Yellow
Write-Host ""
Write-Host "  To stop all services:"
Write-Host "    docker compose down"
Write-Host ""
Write-Host "  To view logs:"
Write-Host "    docker compose logs -f"
Write-Host ""
Write-Host "=============================================="
