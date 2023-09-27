shell:
	bash

test:
	@echo "> Running unit tests"
	dotnet test ./VirtualFinland.UsersAPI.UnitTests --no-restore

prep-deploy: test
	@echo "> Ensuring local dependencies are installed"
	dotnet tool install -g Amazon.Lambda.Tools || true
	@echo "> Building and packaging deployment package for Users API"
	dotnet lambda package --project-location ./VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI
	@echo "> Building and packaging deployment package for database migration runner"
	dotnet lambda package --project-location ./VirtualFinland.UsersAPI.DatabaseMigrationRunner
	zip -d ./VirtualFinland.UsersAPI.DatabaseMigrationRunner/bin/Release/net6.0/VirtualFinland.UsersAPI.DatabaseMigrationRunner.zip "VirtualFinland.UsersAPI.deps.json" || true
	zip -d ./VirtualFinland.UsersAPI.DatabaseMigrationRunner/bin/Release/net6.0/VirtualFinland.UsersAPI.DatabaseMigrationRunner.zip "VirtualFinland.UsersAPI.runtimeconfig.json" || true
	@echo "> Building and packaging deployment package for the audit log subscription function"
	dotnet lambda package --project-location ./VirtualFinland.UsersAPI.AuditLogSubscription

deploy: prep-deploy
	pulumi -C ./VirtualFinland.UsersAPI.Deployment up
