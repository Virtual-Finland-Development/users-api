#!/bin/bash

echo "Starting Users API in docker"
cd ./VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI
echo "Work Directory:"
pwd
echo "Building Docker Image of API"
docker build . -t virtualfinland/usersapi:latest
echo "Starting LocalDev in Docker"
cd ../../../
echo "Work Directory:"
pwd
docker-compose -f ./Tools/Docker/docker-compose-localdev.yml up


