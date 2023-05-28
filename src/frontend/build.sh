#!/bin/bash

#echo the commands
set -x

# Build script for the project

# Copy certificate files


# Build the project
cd certman-ui || exit 1
npm install || exit 1
npm run build || exit 1

cd ..
docker build . -t certman-frontend || exit 1

