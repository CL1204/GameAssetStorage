# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GameAssetStorage/GameAssetStorage.csproj", "GameAssetStorage/"]
RUN dotnet restore "GameAssetStorage/GameAssetStorage.csproj"
COPY . .
WORKDIR "/src/GameAssetStorage"
RUN dotnet build "GameAssetStorage.csproj" -c Release -o /app/build
RUN dotnet publish "GameAssetStorage.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://*:$PORT
ENTRYPOINT ["dotnet", "GameAssetStorage.dll"]