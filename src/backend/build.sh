#!/bin/bash

#echo the commands 
set -x

# Build script for the project

# Clean the project
echo "Cleaning the project..."
dotnet clean || exit 1

# Build the project
echo "Building the project..."
cd certman || exit 1

dotnet build || exit 1

# Cleanup the publish folder
echo "Cleaning the publish folder..."
rm -rf ./bin/Release/net6.0/linux-x64/publish/* || exit 1

# publish in release mode, for linux, and exit if errors #/p:PublishTrimmed=true
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true || exit 1  

#move appsettings.dockerlinux.json to appsettings.json
rm ./bin/Release/net6.0/linux-x64/publish/appsettings.json
mv ./bin/Release/net6.0/linux-x64/publish/appsettings.dockerlinux.json ./bin/Release/net6.0/linux-x64/publish/appsettings.json

cd .. 
docker build . -t certman-backend || exit 1