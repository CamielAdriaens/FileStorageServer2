

# Stage 2: Build and restore dependencies
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY . /source

WORKDIR /source/FileStorage

RUN dotnet build -c Release -o /app/build


# Stage 3: Publish the application (to a separate directory)
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 4: Final runtime image (production)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish . 


ENV ASPNETCORE_URLS=http://+:8080
# Use ENTRYPOINT to run the application when the container starts

ENTRYPOINT ["dotnet", "FileStorage.dll"]
