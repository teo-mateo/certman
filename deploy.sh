#!/bin/bash

docker save -o certman.tar certman | bzip2 | ssh heapzilla@192.168.2.77 docker load 