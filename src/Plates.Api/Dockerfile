FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY global.json Plates.slnx ./
COPY src/Plates.Engine/Plates.Engine.csproj src/Plates.Engine/
COPY src/Plates.Api/Plates.Api.csproj src/Plates.Api/
RUN dotnet restore src/Plates.Api/Plates.Api.csproj
COPY src/ src/
RUN dotnet publish src/Plates.Api/Plates.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "Plates.Api.dll"]
