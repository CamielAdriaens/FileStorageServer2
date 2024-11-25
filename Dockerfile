# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage 2: Build and restore dependencies
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files to their respective directories
COPY ["FileStorage/FileStorage.csproj", "FileStorage/"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["DTOs/DTOs.csproj", "DTOs/"]
COPY ["FileStorage.Tests/FileStorage.Tests.csproj", "FileStorage.Tests/"]
COPY ["INTERFACES/INTERFACES.csproj", "INTERFACES/"]
COPY ["LOGIC/LOGIC.csproj", "LOGIC/"]
COPY ["MODELS/MODELS.csproj", "MODELS/"]

# Restore dependencies for the main project
RUN dotnet restore "FileStorage/FileStorage.csproj"

# Copy all source code and build the application
COPY . .
WORKDIR "/src/FileStorage"
RUN dotnet build -c Release -o /app/build

# Stage 3: Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileStorage.dll"]
