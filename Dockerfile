# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
ENV DOTNET_EnableDiagnostics=0

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# cache de restore
COPY Spark.Api/Spark.Api.csproj Spark.Api/
COPY Spark.Domain/Spark.Domain.csproj Spark.Domain/
COPY Spark.Infra/Spark.Infra.csproj Spark.Infra/
RUN dotnet restore Spark.Api/Spark.Api.csproj

# código
COPY . .

# (opcional) bust cache em cada deploy
ARG BUILD_ID=20250914_2

# publish
RUN dotnet publish Spark.Api/Spark.Api.csproj -c Release -o /app/out /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/out /app/

# Render injeta $PORT; fallback local = 8080
ENTRYPOINT ["sh","-c","dotnet /app/Spark.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
