# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GameAssetStorage.csproj", "."]
RUN dotnet restore "GameAssetStorage.csproj"
COPY . .
RUN dotnet build "GameAssetStorage.csproj" -c Release -o /app/build
RUN dotnet publish "GameAssetStorage.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GameAssetStorage.dll"]