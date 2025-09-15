# syntax=docker/dockerfile:1

#########################
# RUNTIME (.NET 6)
#########################
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
ENV DOTNET_EnableDiagnostics=0

#########################
# BUILD (.NET 6)
#########################
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Cache de restore
COPY Spark.Api/Spark.Api.csproj Spark.Api/
COPY Spark.Domain/Spark.Domain.csproj Spark.Domain/
COPY Spark.Infra/Spark.Infra.csproj Spark.Infra/
RUN dotnet restore Spark.Api/Spark.Api.csproj

# Código
COPY . .

# Publish
RUN dotnet publish Spark.Api/Spark.Api.csproj -c Release -o /app/out /p:UseAppHost=false

#########################
# FINAL
#########################
FROM base AS final
WORKDIR /app
COPY --from=build /app/out .

# Força bind na porta do Render ($PORT) ou 8080 local
ENTRYPOINT ["sh","-c","dotnet Spark.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
