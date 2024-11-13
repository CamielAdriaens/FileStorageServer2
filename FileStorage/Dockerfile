# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all .csproj files individually for dependency restoration
COPY ["FileStorage.csproj", "./"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["DTOs/DTOs.csproj", "DTOs/"]
COPY ["FileStorage/FileStorage.csproj", "FileStorage/"]
COPY ["FileStorage.Tests/FileStorage.Tests.csproj", "FileStorage.Tests/"]
COPY ["INTERFACES/INTERFACES.csproj", "INTERFACES/"]
COPY ["LOGIC/LOGIC.csproj", "LOGIC/"]
COPY ["MODELS/MODELS.csproj", "MODELS/"]

# Restore dependencies for all projects
RUN dotnet restore "./FileStorage.csproj"

# Copy the entire source code into the container
COPY . .

# Set working directory to the main project and build the project
WORKDIR "/src/FileStorage"
RUN dotnet build "./FileStorage.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileStorage.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage for runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables for sensitive configurations
ENV MONGODB_CONNECTION_STRING="mongodb+srv://472595:mongodatabase@clusterfilestorage.en7np.mongodb.net/?retryWrites=true&w=majority&appName=ClusterFileStorage&authMechanism=SCRAM-SHA-1"
ENV MSSQL_CONNECTION_STRING="Server=mssqlstud.fhict.local;Database=dbi472595;User Id=dbi472595;Password=database;MultipleActiveResultSets=true;TrustServerCertificate=True"
ENV GOOGLE_CLIENT_ID="911031744599-l50od06i5t89bmdl4amjjhdvacsdonm7.apps.googleusercontent.com"
ENV JWT_SECRET_KEY="thisisaverysecurelongsecretkey12345"

# Set the entry point for the application
ENTRYPOINT ["dotnet", "FileStorage.dll"]
