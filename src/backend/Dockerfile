FROM ubuntu:latest

# Install openssl
RUN apt-get update && apt-get install -y openssl aspnetcore-runtime-6.0


RUN mkdir /certman
RUN mkdir /certman/app
RUN mkdir /certman/store
RUN mkdir /certman/workdir

# copy all files and directories from the publish folder (./certman/bin/Release/net6.0/publish) to the app folder (/app), excluding any files that start with appsettings
COPY ./certman/bin/Release/net6.0/publish /certman/app
RUN rm /certman/app/appsettings*

COPY ./certman/bin/Release/net6.0/publish/appsettings.dockerlinux.json /certman/app/appsettings.json

# expose port 8081
EXPOSE 8081

WORKDIR /certman/app

# run dotnet /certman/app/certman.dll
CMD ["dotnet", "/certman/app/certman.dll"]
