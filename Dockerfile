

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
ENV DOTNET_EnableDiagnostics=0

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src


COPY . .

RUN dotnet publish Spark.Api/Spark.Api.csproj -c Release -o /app /p:UseAppHost=false

RUN ls -la /app

FROM base AS final
WORKDIR /app
COPY --from=build /app /app


ENTRYPOINT ["sh","-c","dotnet $(ls /app/*.dll) --urls http://0.0.0.0:${PORT:-8080}"]
