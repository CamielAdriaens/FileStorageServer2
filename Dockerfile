# Stage 1: Base runtime image (for production)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Stage 2: Build and restore dependencies
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file and restore dependencies
COPY ["FileStorage.sln", "./"]
COPY ["FileStorage/FileStorage.csproj", "FileStorage/"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["DTOs/DTOs.csproj", "DTOs/"]
COPY ["FileStorage.Tests/FileStorage.Tests.csproj", "FileStorage.Tests/"]
COPY ["INTERFACES/INTERFACES.csproj", "INTERFACES/"]
COPY ["LOGIC/LOGIC.csproj", "LOGIC/"]
COPY ["MODELS/MODELS.csproj", "MODELS/"]

# Restore all the dependencies defined in the solution file
RUN dotnet restore "FileStorage.sln"

# Copy the rest of the code
COPY . .

# Build the application in Release configuration
WORKDIR "/src/FileStorage"
RUN dotnet build -c Release -o /app/build

# Stage 3: Publish the application (to a separate directory)
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 4: Final runtime image (production)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 

# Use ENTRYPOINT to run the application when the container starts
ENTRYPOINT ["dotnet", "FileStorage.dll"]
