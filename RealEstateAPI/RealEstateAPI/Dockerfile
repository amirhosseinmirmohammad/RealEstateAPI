FROM delta.ir/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM delta.ir/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["RealEstateAPI/RealEstateService.csproj", "RealEstateAPI/"]
COPY ["RealEstateApplication/RealEstateApplication.csproj", "RealEstateApplication/"]
COPY ["RealEstateCore/RealEstateCore.csproj", "RealEstateCore/"]
COPY ["RealEstateInfrastructure/RealEstateInfrastructure.csproj", "RealEstateInfrastructure/"]

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "./RealEstateAPI/RealEstateService.csproj"

COPY . .

WORKDIR "/src/RealEstateAPI"
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet build "./RealEstateService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RealEstateService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RealEstateService.dll"]
