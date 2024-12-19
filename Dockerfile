

# Stage 2: Build and restore dependencies
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY . /source

WORKDIR /source/FileStorage

RUN dotnet build -c Release -o /app/build
ARG CERT_PATH=/tmp/https/aspnet
RUN mkdir -p ${CERT_PATH}


# Generate HTTPS certificate in the build stage
RUN dotnet dev-certs https -v -ep ${CERT_PATH}/aspnetapp.pfx -p "zDiojwa398DSA" && \
    ls -la ${CERT_PATH}

# Stage 3: Publish the application (to a separate directory)
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 4: Final runtime image (production)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish . 

ARG CERT_PATH=/tmp/https/aspnet

# Copy everything needed to run the app from the "build" stage.
COPY --from=build ${CERT_PATH}/aspnetapp.pfx /https/aspnetapp.pfx


# Change permissions to make the certificate accessible to the non-root user
RUN chown $APP_UID:$APP_UID /https/aspnetapp.pfx && chmod 644 /https/aspnetapp.pfx

# Switch to a non-privileged user (defined in the base image) that the app will run under.
# See https://docs.docker.com/go/dockerfile-user-best-practices/
# and https://github.com/dotnet/dotnet-docker/discussions/4764

USER $APP_UID

ENV ASPNETCORE_URLS=https://+:7058
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="zDiojwa398DSA"
# Use ENTRYPOINT to run the application when the container starts

ENTRYPOINT ["dotnet", "FileStorage.dll"]
