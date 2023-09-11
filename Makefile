shell:
	bash

test:
	@echo "> Running unit tests"
	dotnet test ./VirtualFinland.UsersAPI.UnitTests --no-restore

build:
	dotnet build

migrate: build
	@echo "> Running database migrations"
	dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI migrate

update-terms-of-service: build
	@echo "> Updating terms of service in database"
	dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI update-terms-of-service

packages: test
	@echo "> Ensuring local dependencies are installed"
	dotnet tool install -g Amazon.Lambda.Tools || true
	@echo "> Building and packaging deployment package for Users API"
	dotnet lambda package --project-location ./VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI
	@echo "> Building and packaging deployment package for database migration runner"
	dotnet lambda package --project-location ./VirtualFinland.UsersAPI.AdminFunction
	zip -d ./VirtualFinland.UsersAPI.AdminFunction/bin/Release/net6.0/VirtualFinland.UsersAPI.AdminFunction.zip "VirtualFinland.UsersAPI.deps.json" || true
	zip -d ./VirtualFinland.UsersAPI.AdminFunction/bin/Release/net6.0/VirtualFinland.UsersAPI.AdminFunction.zip "VirtualFinland.UsersAPI.runtimeconfig.json" || true

deploy: packages
	pulumi -C ./VirtualFinland.UsersAPI.Deployment up
