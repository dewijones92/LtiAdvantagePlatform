#!/bin/bash
set -e
# Start the SQL Server container
git clean -xdf
#docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YourStrong@Passw0rd>" -p 1433:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2022-latest

exec ub docker run --name lti-platform --rm --network host -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/core/sdk:2.2 /bin/bash -c "dotnet clean && dotnet build && dotnet run" |& ub tsm  |& ub tee /home/ltiuser/logs/logs/dotnet_logs
