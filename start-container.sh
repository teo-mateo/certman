#!/bin/bash

image=certman:$(docker images | grep certman | awk '{print $2}' | sort -n | tail -1)

docker run -d \
-p 5050:5050 \
-p 5051:5051 \
-v /volume1/docker/apps/certman/volumes/data:/certman/data \
-w /certman/app \
--name certman_app \
-e REACT_APP_SERVER_URL=https://netstorage.gogu:5051 \
-e ASPNETCORE_WEBROOT=/certman/webroot \
$image
