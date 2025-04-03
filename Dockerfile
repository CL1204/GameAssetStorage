# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "GameAssetStorage.csproj"
RUN dotnet publish "GameAssetStorage.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://*:$PORT
EXPOSE $PORT
ENTRYPOINT ["dotnet", "GameAssetStorage.dll"]