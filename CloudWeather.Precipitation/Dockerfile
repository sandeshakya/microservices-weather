# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /src
COPY . .
RUN dotnet restore "./CloudWeather.Precipitation.csproj" --disable-parallel
RUN dotnet publish "./CloudWeather.Precipitation.csproj" -c release -o /publish --no-restore

# Serve Stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 as final
WORKDIR /app
COPY --from=build /publish ./

EXPOSE 5000

ENTRYPOINT [ "dotnet", "CloudWeather.Precipitation.dll" ]