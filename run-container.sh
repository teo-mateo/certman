﻿#!/usr/bin/env bash

#kill container if it's running
docker kill certman_app

#remove container if it exists
docker rm certman_app

# create folder ~/certmandata if it doesn't exist
# -p flag creates parent directories if they don't exist
mkdir -p ~/certmandata


#run container
docker run -d \
-p 5050:5050 \
-p 5051:5051 \
-v ~/certmandata:/certman/data \
-w /certman/app \
--name certman_app \
certman