#!/bin/bash

docker save -o certman.tar certman || exit 1
bzip2 certman.tar || exit 1

# scp to heapzilla@netstorage.gogu
scp certman.tar.bz2 heapzilla@netstorage.gogu:/volume1/docker/apps/certman
