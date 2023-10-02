shell:
	bash

test:
	@echo "> Running unit tests"
	dotnet test ./VirtualFinland.UsersAPI.UnitTests --no-restore

migrate: build
	@echo "> Running database migrations"
	dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI migrate

packages: test
	@echo "> Ensuring local dependencies are installed"
	dotnet tool install -g Amazon.Lambda.Tools || true
	mkdir -p build-artifacts
	@echo "> Building and packaging deployment package for Users API"
	dotnet lambda package --project-location ./VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI --output-package ./build-artifacts/VirtualFinland.UsersAPI.zip
	@echo "> Building and packaging deployment package for the admin functions"
	dotnet lambda package --project-location ./VirtualFinland.UsersAPI.AdminFunction --output-package ./build-artifacts/VirtualFinland.UsersAPI.AdminFunction.zip

deploy: packages
	pulumi -C ./VirtualFinland.UsersAPI.Deployment up
