# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia os arquivos de solução e projeto primeiro (para cache de layers)
COPY StoreSyncBack/StoreSyncBack.csproj StoreSyncBack/
COPY SharedModels/SharedModels/SharedModels.csproj SharedModels/SharedModels/

# Restaura as dependências
RUN dotnet restore StoreSyncBack/StoreSyncBack.csproj

# Copia o restante do código
COPY . .

# Builda e publica
RUN dotnet publish StoreSyncBack/StoreSyncBack.csproj -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
ENV TZ=America/Sao_Paulo
WORKDIR /app

# Copia os arquivos publicados
COPY --from=build /app/publish .

# Expõe as portas
EXPOSE 8080
EXPOSE 8081

# Ponto de entrada
ENTRYPOINT ["dotnet", "StoreSyncBack.dll"]
