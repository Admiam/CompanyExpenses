#!/bin/bash
# =====================================================
# Company Expenses - Docker Startup Script
# This script starts all services with a single command
# =====================================================

set -e

echo ""
echo "=============================================="
echo "  Company Expenses - Docker Setup"
echo "=============================================="
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "Error: Docker is not installed."
    echo "Please install Docker from https://docker.com"
    exit 1
fi

# Check if Docker Compose is available
if ! docker compose version &> /dev/null; then
    echo "Error: Docker Compose is not available."
    echo "Please install Docker Compose or update Docker."
    exit 1
fi

echo "Starting all services..."
echo ""

# Build and start all containers
docker compose up --build -d

echo ""
echo "=============================================="
echo "  Waiting for services to be ready..."
echo "=============================================="
echo ""

# Wait for database to be healthy
echo "Waiting for database..."
timeout=120
counter=0
until docker compose exec -T db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "CompanyExpenses123!" -C -Q "SELECT 1" &> /dev/null; do
    counter=$((counter + 1))
    if [ $counter -ge $timeout ]; then
        echo "Timeout waiting for database."
        exit 1
    fi
    sleep 1
    echo -n "."
done
echo " Ready!"

# Wait a bit more for apps to start
echo "Waiting for applications to initialize..."
sleep 15

echo ""
echo "=============================================="
echo "  Company Expenses is now running!"
echo "=============================================="
echo ""
echo "  Frontend App:   http://localhost:3000"
echo "  Auth Server:    http://localhost:7169"
echo "  API Server:     http://localhost:5200"
echo ""
echo "  Admin Login Credentials:"
echo "    Email:    admin@company-expenses.local"
echo "    Password: Admin123!"
echo ""
echo "  To stop all services:"
echo "    docker compose down"
echo ""
echo "  To view logs:"
echo "    docker compose logs -f"
echo ""
echo "=============================================="
