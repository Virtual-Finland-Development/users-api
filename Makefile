shell:
	bash

test:
	@echo "> Running unit tests"
	dotnet test ./VirtualFinland.UsersAPI.UnitTests --no-restore

prep-deploy: test
	@echo "> Ensuring local dependencies are installed"
	dotnet tool install -g Amazon.Lambda.Tools || true
	mkdir -p build-artifacts
	@echo "> Building and packaging deployment package for Users API"
	dotnet lambda package --project-location ./VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI --output-package ./build-artifacts/VirtualFinland.UsersAPI.zip
	@echo "> Building and packaging deployment package for database migration runner"
	dotnet lambda package --project-location ./VirtualFinland.UsersAPI.DatabaseMigrationRunner --output-package ./build-artifacts/VirtualFinland.UsersAPI.DatabaseMigrationRunner.zip
	@echo "> Building and packaging deployment package for the audit log subscription function"
	dotnet lambda package --project-location ./VirtualFinland.UsersAPI.AuditLogSubscription --output-package ./build-artifacts/VirtualFinland.UsersAPI.AuditLogSubscription.zip

deploy: prep-deploy
	pulumi -C ./VirtualFinland.UsersAPI.Deployment up
