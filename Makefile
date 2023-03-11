shell:
	bash

test:
	dotnet test ./VirtualFinland.UsersAPI.UnitTests --no-restore

prep-deploy: test
	dotnet tool install -g Amazon.Lambda.Tools || true
	dotnet lambda package --project-location ./VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI

deploy: prep-deploy
	pulumi -C ./VirtualFinland.UsersAPI.Deployment up --yes
