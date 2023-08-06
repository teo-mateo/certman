FROM ubuntu:latest

# Install openssl and aspnetcore-runtime-6.0
RUN apt-get update && apt-get install -y openssl aspnetcore-runtime-6.0

# Install curl
RUN apt-get update && apt-get install -y curl

RUN mkdir /certman
RUN mkdir /certman/app

# The directory /certman/data must be mapped to a volume
RUN mkdir /certman/data
RUN mkdir /certman/data/store
RUN mkdir /certman/data/workdir
RUN mkdir /certman/webroot

# copy all files and directories from the publish folder (./certman/bin/Release/net6.0/publish) to the app folder (/app), excluding any files that start with appsettings
COPY ./src/backend/certman-server/certman/bin/Release/net6.0/linux-x64/publish /certman/app

# copy Certman-prod.pfx to the app folder (/certman/app) and rename it to Certman.pfx
COPY ./certificate/Certman-prod.pfx /certman/app/Certman.pfx

# copy all files from the frontend build folder (./certman/src/frontend/certman-client/dist) to the webroot folder (/certman/webroot)
COPY ./src/frontend/certman-ui/build /certman/webroot

# Set environment variables
# ASPNETCORE_WEBROOT is the path to the webroot folder

# Copy environment variables file and source it
COPY ./.env /certman

# Copy entrypoint script
COPY ./entrypoint.sh /certman
RUN chmod +x /certman/entrypoint.sh

EXPOSE 5050
EXPOSE 5051

WORKDIR /certman/app

# Run the app
CMD ["/certman/entrypoint.sh"]
