FROM node:24-alpine AS web
WORKDIR /web
COPY web/package.json web/package-lock.json web/.npmrc ./
RUN npm ci
COPY web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY global.json Directory.Build.props Directory.Packages.props Zad.slnx BannedSymbols.txt .editorconfig ./
COPY src ./src
COPY tools ./tools

RUN dotnet publish src/Zad.Host/Zad.Host.csproj -c Release -o /app --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app ./
COPY --from=web /web/dist/app/browser ./wwwroot

ENTRYPOINT ["dotnet", "Zad.Host.dll"]
