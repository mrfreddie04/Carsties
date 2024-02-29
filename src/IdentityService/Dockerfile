# Note! will run docker file from the solution root folder

# specify base image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app

EXPOSE 80

# copy all .csproj files and restore as distinct layers 
# use the same COPY command for ALL Dockerfiles to take advantage of docker caching
COPY Carsties.sln ./Carsties.sln
COPY src/AuctionService/AuctionService.csproj ./src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj ./src/SearchService/SearchService.csproj
COPY src/Contracts/Contracts.csproj ./src/Contracts/Contracts.csproj
COPY src/GatewayService/GatewayService.csproj ./src/GatewayService/GatewayService.csproj
COPY src/IdentityService/IdentityService.csproj ./src/IdentityService/IdentityService.csproj
COPY src/BiddingService/BiddingService.csproj ./src/BiddingService/BiddingService.csproj
COPY src/NotificationService/NotificationService.csproj ./src/NotificationService/NotificationService.csproj
COPY test/AuctionService.UnitTests/AuctionService.UnitTests.csproj ./test/AuctionService.UnitTests/AuctionService.UnitTests.csproj
COPY test/AuctionService.IntegrationTests/AuctionService.IntegrationTests.csproj ./test/AuctionService.IntegrationTests/AuctionService.IntegrationTests.csproj

# Restore package dependencies for the entire solution (install dependencies)
RUN dotnet restore Carsties.sln

# Copy app src files 
COPY src/IdentityService ./src/IdentityService

# Switch to IdentityService project directory & build the project in the release mode
WORKDIR /app/src/IdentityService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image - switch to dotnet runtime base image (smaller & faster then sdk)
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS release
# Copy release files over from "build" image
WORKDIR /app
COPY --from=build /app/src/out .

# Startup command - start Identity Service
ENTRYPOINT ["dotnet", "IdentityService.dll"]