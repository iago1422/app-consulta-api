# syntax=docker/dockerfile:1

# Runtime (imagem leve)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Apenas dica; o Render injeta $PORT em runtime
EXPOSE 8080

# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia tudo (simples e seguro se você não quer listar cada .csproj)
COPY . .

# Restaura e publica a API
RUN dotnet restore ./Spark.Api/Spark.Api.csproj
RUN dotnet publish ./Spark.Api/Spark.Api.csproj -c Release -o /app/out /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=build /app/out .

# Faz a API escutar na porta que o Render fornecer.
# Localmente, se $PORT não existir, usa 8080.
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["sh","-c","dotnet Spark.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
