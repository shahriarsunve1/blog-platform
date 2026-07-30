FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BlogAPI.Domain/*.csproj BlogAPI.Domain/
COPY BlogAPI.Data/*.csproj BlogAPI.Data/
COPY BlogAPI.Core/*.csproj BlogAPI.Core/
COPY BlogAPI.API/*.csproj BlogAPI.API/
RUN dotnet restore BlogAPI.API/BlogAPI.API.csproj

COPY . .
RUN dotnet publish BlogAPI.API/BlogAPI.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "BlogAPI.API.dll"]
