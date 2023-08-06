#!/bin/bash

# first script argument is the image certman:tag
image=$1

#if no image is provided, exit
if [ -z "$image" ]
then
    echo "No image provided. Usage: deploy.sh certman:tag"
    exit 1
fi

# check if image exists using docker images --filter and if no such image exists, exit
if ! docker images --format "{{.Repository}}:{{.Tag}}" | grep $image
then
    echo "Image $image does not exist. Exiting..."
    exit 1
fi

# get container with highest tag
latest=certman:$(docker images | grep certman | awk '{print $2}' | sort -n | tail -1)

#echo the commands 
set -x

size=$(docker save $image | wc -c)

docker save $image | pv -s $size | bzip2 | ssh heapzilla@netstorage.gogu "/usr/local/bin/docker load"

ssh heapzilla@netstorage.gogu << EOF

docker stop certman_app
docker rm certman_app
cd /volume1/docker/apps/certman
./start-container.sh

EOF