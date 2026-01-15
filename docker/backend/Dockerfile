FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Backend/CaraDogBackend.sln ./Backend/
COPY Backend/CaraDog.Api/CaraDog.Api.csproj Backend/CaraDog.Api/
COPY Backend/CaraDog.Core/CaraDog.Core.csproj Backend/CaraDog.Core/
COPY Backend/CaraDog.Db/CaraDog.Db.csproj Backend/CaraDog.Db/
COPY Backend/CaraDog.DTO/CaraDog.DTO.csproj Backend/CaraDog.DTO/

RUN dotnet restore Backend/CaraDog.Api/CaraDog.Api.csproj

COPY Backend/ Backend/
WORKDIR /src/Backend/CaraDog.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CaraDog.Api.dll"]
