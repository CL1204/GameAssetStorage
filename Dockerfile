# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy just the project file and restore
COPY ["GameAssetStorage.csproj", "."]
RUN dotnet restore "GameAssetStorage.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "GameAssetStorage.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://*:$PORT
EXPOSE 10000

ENTRYPOINT ["dotnet", "GameAssetStorage.dll"]
