FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OrgUnitAPI.sln ./
COPY src/OrgUnitAPi/OrgUnitAPI.csproj src/OrgUnitAPi/
RUN dotnet restore src/OrgUnitAPi/OrgUnitAPI.csproj

COPY src ./src
RUN dotnet publish src/OrgUnitAPi/OrgUnitAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OrgUnitAPI.dll"]
