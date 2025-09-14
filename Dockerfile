# syntax=docker/dockerfile:1

#########################
# RUNTIME (.NET 6)
#########################
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080

# Garante que a app escute na porta do Render ($PORT) ou 8080 local
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENV DOTNET_EnableDiagnostics=0

#########################
# BUILD (.NET 6)
#########################
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copia apenas csprojs primeiro (melhor cache)
COPY Spark.Api/Spark.Api.csproj Spark.Api/
COPY Spark.Domain/Spark.Domain.csproj Spark.Domain/
COPY Spark.Infra/Spark.Infra.csproj Spark.Infra/

# (Se tiver .sln, você pode copiar e restaurar por ele)
# COPY Spark.sln ./

RUN dotnet restore Spark.Api/Spark.Api.csproj

# Agora copia o restante do código
COPY . .

# Publica (sem host nativo)
RUN dotnet publish Spark.Api/Spark.Api.csproj -c Release -o /app/out /p:UseAppHost=false

#########################
# FINAL
#########################
FROM base AS final
WORKDIR /app
COPY --from=build /app/out .

# Start direto (porta já configurada via ASPNETCORE_URLS)
ENTRYPOINT ["dotnet", "Spark.Api.dll"]
