FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ToursAyacuchoPeruAPI/ToursAyacuchoPeruAPI.csproj ToursAyacuchoPeruAPI/
RUN dotnet restore ToursAyacuchoPeruAPI/ToursAyacuchoPeruAPI.csproj

COPY ToursAyacuchoPeruAPI/ ToursAyacuchoPeruAPI/
RUN dotnet publish ToursAyacuchoPeruAPI/ToursAyacuchoPeruAPI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:5150
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5150

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ToursAyacuchoPeruAPI.dll"]
