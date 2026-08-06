FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore TopHeroesBot.sln
RUN dotnet publish TopHeroesBot.Bot/TopHeroesBot.Bot.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TopHeroesBot.Bot.dll"]