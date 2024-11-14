# Start with the base image for .NET 8.0 runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Use SDK image for .NET 8.0 to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy each .csproj file to its specific folder in the build context
COPY ["FileStorage/FileStorage.csproj", "./"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["DTOs/DTOs.csproj", "DTOs/"]
COPY ["FileStorage/FileStorage.csproj", "FileStorage/"]
COPY ["FileStorage.Tests/FileStorage.Tests.csproj", "FileStorage.Tests/"]
COPY ["INTERFACES/INTERFACES.csproj", "INTERFACES/"]
COPY ["LOGIC/LOGIC.csproj", "LOGIC/"]
COPY ["MODELS/MODELS.csproj", "MODELS/"]

# Restore dependencies for the main project
RUN dotnet restore "FileStorage.csproj"

# Copy all source files and build the application
COPY . .
WORKDIR "/src/FileStorage"
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final stage with the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileStorage.dll"]