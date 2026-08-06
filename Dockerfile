FROM mcr.microsoft.com/playwright/dotnet:v1.54.0-noble AS build

WORKDIR /src

COPY . .

RUN dotnet restore TopHeroesBot.sln
RUN dotnet publish TopHeroesBot.Bot/TopHeroesBot.Bot.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/playwright/dotnet:v1.54.0-noble

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TopHeroesBot.Bot.dll"]