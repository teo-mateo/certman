FROM ubuntu:latest

# Install openssl
RUN apt-get update && apt-get install -y openssl aspnetcore-runtime-6.0


RUN mkdir /certman
RUN mkdir /certman/app

# The directory /certman/data must be mapped to a volume
RUN mkdir /certman/data
RUN mkdir /certman/data/store
RUN mkdir /certman/data/workdir

# copy all files and directories from the publish folder (./certman/bin/Release/net6.0/publish) to the app folder (/app), excluding any files that start with appsettings
COPY ./certman/bin/Release/net6.0/linux-x64/publish /certman/app

# copy the certificate
COPY ./certman/Certman.pfx /certman/app

EXPOSE 5050
EXPOSE 5051

WORKDIR /certman/app

# Run the app
CMD ["/certman/app/certman"]
