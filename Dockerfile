# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

#ENV ASPNETCORE_HTTP_PORTS 8080

# Use the SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CorrePalabras.csproj", "./"]
RUN dotnet restore "./CorrePalabras.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CorrePalabras.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CorrePalabras.csproj" -c $BUILD_CONFIGURATION -o /app/publish

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CorrePalabras.dll", "--environment=Development"]