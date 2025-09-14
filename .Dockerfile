# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copia csprojs primeiro (cache)
COPY Spark.Domain/Spark.Domain.csproj Spark.Domain/
COPY Spark.Infra/Spark.Infra.csproj   Spark.Infra/
COPY SparkApi/SparkApi.csproj         SparkApi/

RUN dotnet restore SparkApi/SparkApi.csproj

# Copia o restante
COPY . .
RUN dotnet publish SparkApi/SparkApi.csproj -c Release -o /app/publish --no-restore

# ===== runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0

COPY --from=build /app/publish ./

# Render injeta a porta em $PORT
EXPOSE 8080
CMD ["sh","-c","dotnet SparkApi.dll --urls http://0.0.0.0:${PORT}"]
