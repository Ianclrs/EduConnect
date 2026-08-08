FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (layer caching)
COPY src/EduGestor.Core/EduGestor.Core.csproj src/EduGestor.Core/
COPY src/EduGestor.Infrastructure/EduGestor.Infrastructure.csproj src/EduGestor.Infrastructure/
COPY src/EduGestor.Api/EduGestor.Api.csproj src/EduGestor.Api/
RUN dotnet restore src/EduGestor.Api/EduGestor.Api.csproj

# Copy all source and build
COPY . .
RUN dotnet publish src/EduGestor.Api/EduGestor.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "EduGestor.Api.dll"]
