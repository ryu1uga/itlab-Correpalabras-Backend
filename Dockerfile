FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["CorrePalabras.csproj", "./"]

RUN dotnet restore "./CorrePalabras.csproj"

COPY . .

RUN dotnet build "CorrePalabras.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "CorrePalabras.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

RUN chmod -R g=u /app

USER 1001

ENTRYPOINT ["dotnet", "CorrePalabras.dll"]