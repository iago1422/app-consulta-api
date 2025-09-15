# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
ENV DOTNET_EnableDiagnostics=0

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# c�pia simples (sem cache pra n�o confundir)
COPY . .

# for�a rebuild entre deploys
ARG BUILD_ID=20250914_04

# publish
RUN dotnet publish Spark.Api/Spark.Api.csproj -c Release -o /app/out /p:UseAppHost=false

# s� pra confer�ncia no log
RUN ls -la /app/out

FROM base AS final
WORKDIR /app
COPY --from=build /app/out /app/

# roda a SUA dll (nome exato) e for�a bind no Render
ENTRYPOINT ["sh","-c","exec dotnet /app/Spark.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
