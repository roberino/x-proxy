#!/bin/sh

docker build -t xproxy -f Dockerfile.web .

# docker stop <already-running-container>

docker run -i -p 8092:8092 -p 9373:9373 -p 8080:8080  -e "TARGET_HOST=$1" xproxy
