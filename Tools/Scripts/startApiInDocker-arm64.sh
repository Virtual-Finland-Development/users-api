#!/bin/bash
export USERAPI_DOCKERFILE=Dockerfile.arm64
docker compose -f ./docker-compose.yml up --build
