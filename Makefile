shell:
	bash

restore:
	@echo "> Restoring dependencies"
	dotnet restore ./VirtualFinland.UsersAPI.sln

test: restore
	@echo "> Running unit tests"
	dotnet test ./VirtualFinland.UsersAPI.UnitTests --no-restore

migrate.cli:
	@echo "> Running database migrations"
	dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI migrate
migrate.aws:
	@echo "> Running database migrations in AWS Lambda"
	aws lambda invoke --payload '{"Action": "Migrate"}' --cli-binary-format raw-in-base64-out --function-name $(pulumi -C ./VirtualFinland.UsersAPI.Deployment stack output AdminFunctionArn) output.json


update-terms-of-service:
	@echo "> Updating terms of service in database"
	dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI update-terms-of-service

invalidate-caches:
	@echo "> Invalidating caches"
	dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI invalidate-caches

packages: test
	@echo "> Ensuring local dependencies are installed"
	dotnet tool install -g Amazon.Lambda.Tools || true
	mkdir -p build-artifacts
	@echo "> Building and packaging deployment package for Users API"
	dotnet lambda package --project-location ./VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI --output-package ./build-artifacts/VirtualFinland.UsersAPI.zip
	@echo "> Building and packaging deployment package for the admin functions"
	dotnet lambda package --project-location ./VirtualFinland.UsersAPI.AdminFunction --output-package ./build-artifacts/VirtualFinland.UsersAPI.AdminFunction.zip
	@echo "> Building and packaging deployment package for the audit log subscription function"
	dotnet lambda package --project-location ./VirtualFinland.UsersAPI.AuditLogSubscription --output-package ./build-artifacts/VirtualFinland.UsersAPI.AuditLogSubscription.zip

deploy: packages
	pulumi -C ./VirtualFinland.UsersAPI.Deployment up
