#!/bin/bash

# Start the SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YourStrong@Passw0rd>" -p 1433:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2022-latest

# Function to clean up the dotnet container
cleanup() {
    echo "Stopping dotnet container..."
    docker stop $DOTNET_CONTAINER_ID
    exit 0
}

# Trap SIGINT (Ctrl+C) and call the cleanup function
trap cleanup SIGINT

# Run the dotnet container
git clean -xdf
DOTNET_CONTAINER_ID=$(docker run --rm --network host -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/core/sdk:2.2 /bin/bash -c "dotnet clean && dotnet build && dotnet run" & echo $!)

# Wait for the dotnet container to finish
wait $DOTNET_CONTAINER_ID
