# Use the ASP.NET Core runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK for building the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/FileStorage/FileStorage.csproj", "FileStorage/"]
RUN dotnet restore "FileStorage/FileStorage.csproj"
COPY . .
WORKDIR "/src/FileStorage"
RUN dotnet build "FileStorage.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "FileStorage.csproj" -c Release -o /app/publish

# Final stage with the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileStorage.dll"]
