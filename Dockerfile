# syntax=docker/dockerfile:1

# Runtime .NET 6
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080

# Build .NET 6
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copia o repo
COPY . .

# Restaura e publica a API
RUN dotnet restore ./Spark.Api/Spark.Api.csproj
RUN dotnet publish ./Spark.Api/Spark.Api.csproj -c Release -o /app/out /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=build /app/out .

# Usa a porta do Render ($PORT); localmente cai em 8080
ENTRYPOINT ["sh","-c","dotnet Spark.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
