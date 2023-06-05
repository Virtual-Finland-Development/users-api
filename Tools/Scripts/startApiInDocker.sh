#!/bin/bash
set -e # exit on error
echo "Starting Users API in docker"
cd ./VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI
echo "Work Directory:"
pwd
echo "Building Docker Image of API"
docker build -t virtualfinland/usersapi:latest .
echo "Starting LocalDev in Docker"
cd ../../../
echo "Work Directory:"
pwd
docker compose -f ./docker-compose.yml up
docker compose -f ./docker-compose.yml down
