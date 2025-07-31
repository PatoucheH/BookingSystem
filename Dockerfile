FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BookingSystem.csproj", "./"]
RUN dotnet restore "BookingSystem.csproj"
COPY . .
RUN dotnet build "BookingSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookingSystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Copier explicitement le fichier appsettings.Docker.json
COPY appsettings.Docker.json .
ENTRYPOINT ["dotnet", "BookingSystem.dll"]