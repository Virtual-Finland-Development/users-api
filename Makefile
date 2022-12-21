
run:
	docker compose -f Tools/Docker/docker-compose-localdev.yml up --build
run-arm64:
	VF_LOCAL_DOCKERFILE=Dockerfile.arm64 make run
