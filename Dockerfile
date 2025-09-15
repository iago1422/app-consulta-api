# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
ENV DOTNET_EnableDiagnostics=0

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
ARG BUILD_ID=force_rebuild_001
RUN dotnet publish Spark.Api/Spark.Api.csproj -c Release -o /app/out /p:UseAppHost=false
RUN ls -la /app/out

FROM base AS final
WORKDIR /app
COPY --from=build /app/out /app/
ENTRYPOINT ["dotnet", "/app/Spark.Api.dll"]
