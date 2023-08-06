#!/bin/bash

#echo the commands 
set -x

# Build script for the project

# Clean the project
#echo "Cleaning the project..."
#dotnet clean || exit 1

# Save current dir in variable
BUILD_ROOT=$PWD

# Build the project
echo "Building the project..."
cd $BUILD_ROOT/src/backend/certman-server/certman || exit 1

dotnet build || exit 1

# Cleanup the publish folder
echo "Cleaning the publish folder..."
rm -rf ./bin/Release/net6.0/linux-x64/publish/* || exit 1

# publish in release mode, for linux, and exit if errors #/p:PublishTrimmed=true
dotnet publish -c Release -r linux-x64 --self-contained || exit 1  

#move appsettings.dockerlinux.json to appsettings.json
rm ./bin/Release/net6.0/linux-x64/publish/appsettings.json
mv ./bin/Release/net6.0/linux-x64/publish/appsettings.dockerlinux.json ./bin/Release/net6.0/linux-x64/publish/appsettings.json


# Build the frontend
cd $BUILD_ROOT/src/frontend/certman-ui || exit 1

# if first argument is 'dev' run build:dev else run build
if [ "$1" = "prod" ]; then
    echo "Building the frontend in production mode..."
    npm run build:prod || exit 1
else
    echo "Building the frontend in development mode..."
    npm run build:dev || exit 1
fi

cd $BUILD_ROOT

current_version=$(docker images | grep certman | awk '{print $2}' | sort -n | tail -1)
new_version=$((current_version + 1))
echo "Current version: $current_version"
echo "New version: $new_version"

docker build --build-arg VERSION=$new_version -t certman:$new_version . || exit 1