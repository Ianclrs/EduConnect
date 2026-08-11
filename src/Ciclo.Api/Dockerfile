FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (layer caching)
COPY src/Ciclo.Core/Ciclo.Core.csproj src/Ciclo.Core/
COPY src/Ciclo.Infrastructure/Ciclo.Infrastructure.csproj src/Ciclo.Infrastructure/
COPY src/Ciclo.Api/Ciclo.Api.csproj src/Ciclo.Api/
RUN dotnet restore src/Ciclo.Api/Ciclo.Api.csproj

# Copy all source and build
COPY . .
RUN dotnet publish src/Ciclo.Api/Ciclo.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Ciclo.Api.dll"]
