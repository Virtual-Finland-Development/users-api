#!/bin/bash

echo "Starting Users API in docker"
cd ./VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI
echo "Work Directory:"
pwd
echo "Building Docker Image of API"
export USERAPI_DOCKERFILE=Dockerfile.arm64

docker build -f ${USERAPI_DOCKERFILE} -t virtualfinland/usersapi:latest .
echo "Starting LocalDev in Docker"
cd ../../../
echo "Work Directory:"
pwd
docker compose -f ./docker-compose.yml up
docker compose -f ./docker-compose.yml down
