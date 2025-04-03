# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy JUST the project file first
COPY ["GameAssetStorage.csproj", "."]
RUN dotnet restore "GameAssetStorage.csproj"
# Now copy everything else
COPY . .
RUN dotnet build "GameAssetStorage.csproj" -c Release -o /app/build
RUN dotnet publish "GameAssetStorage.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://*:$PORT
ENTRYPOINT ["dotnet", "GameAssetStorage.dll"]