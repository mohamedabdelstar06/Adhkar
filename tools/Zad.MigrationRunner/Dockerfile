FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY global.json Directory.Build.props Directory.Packages.props Zad.slnx BannedSymbols.txt .editorconfig ./
COPY src ./src
COPY tools ./tools

RUN dotnet publish tools/Zad.MigrationRunner/Zad.MigrationRunner.csproj -c Release -o /app --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app ./

ENTRYPOINT ["dotnet", "Zad.MigrationRunner.dll"]
