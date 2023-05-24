#!/bin/bash

docker run -it --rm \
-p 5050:5050 \
-p 5051:5051 \
-v ~/certmandata:/certman/data \
-w /certman/app \
certman-backend