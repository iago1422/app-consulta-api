# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
ENV DOTNET_EnableDiagnostics=0

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# cópia simples (sem cache pra não confundir)
COPY . .

# força rebuild entre deploys
ARG BUILD_ID=20250914_04

# publish
RUN dotnet publish Spark.Api/Spark.Api.csproj -c Release -o /app/out /p:UseAppHost=false

# só pra conferência no log
RUN ls -la /app/out

FROM base AS final
WORKDIR /app
COPY --from=build /app/out /app/

# roda a SUA dll (nome exato) e força bind no Render
ENTRYPOINT ["sh","-c","exec dotnet /app/Spark.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
