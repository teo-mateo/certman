#!/bin/bash

docker save certman | bzip2 | pv | ssh heapzilla@192.168.2.77 docker load 