# Admin function

The administration tool [../VirtualFinland.UsersAPI.AdminFunction](../VirtualFinland.UsersAPI.AdminFunction) is a AWS Lambda function that can be used to perform administrative tasks such as executing database migrations in the protected VPC (Virtual Private Cloud) network. The function is deployed using the [../VirtualFinland.UsersAPI.Deployment](../VirtualFinland.UsersAPI.Deployment) project.

## How to run the admin function locally against a live system:

With correct aws credentials in place the admin function can be invoked using the aws cli, for example a database migration can be run with the following command:

```
aws lambda invoke --payload '{"Action": "Migrate"}' --cli-binary-format raw-in-base64-out --function-name $(pulumi stack output AdminFunctionArn) output.json
```

- where the `--payload` is the json payload that is passed to the function
- where the `--function-name` is the name of the function that is deployed to AWS, this can be found from the pulumi stack outputs: `AdminFunctionArn`

The payload of the function is a json object that contains the following properties:

- `action` - the action that is performed by the function, for example `migrate`
- `data` - an optional stringified data set or a value that is passed to the action implementation

## How to run the admin function locally against a local database:

The setup does not emulate aws lambda-runtime locally but instead runs the admin function as a normal dotnet core console application [../VirtualFinland.UsersAPI.AdminFunction.CLI]([../VirtualFinland.UsersAPI.AdminFunction.CLI)

For example the admin functions migrate-command can be run locally against a local database using the following command:

```
dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI migrate
```

## Available actions

- `migrate` - runs the database migrations
  - lambda function payload: `{"Action": "Migrate"}`
  - cli command: `dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI migrate`
- `initialize-database-audit-log-triggers` - initializes the database audit logging triggers
  - lambda function payload: `{"Action": "InitializeDatabaseAuditLogTriggers"}`
  - cli command: `dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI initialize-database-audit-log-triggers`
- `initialize-database-user` - setup the application-level user credentials to the database
  - lambda function payload: `{"Action": "InitializeDatabaseUser", "data": "{\"Username\": \"appuser\", \"Password\": \"pass\"}"}`
  - cli command: `DATABASE_USER=appuser DATABASE_PASSWORD=pass dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI initialize-database-user`
- `update-terms-of-service`:
  - lambda function payload: `{"Action": "UpdateTermsOfService"}`
  - cli command: `dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI update-terms-of-service`
  - read more at [./README.terms-of-service.md](./README.terms-of-service.md) document
- `update-analytics`:
  - lambda function payload: `{"Action": "UpdateAnalytics"}`
- `invalidate-caches`: invalidates the api gateway caches
  - lambda function payload: `{"Action": "InvalidateCaches"}`
  - cli command: `dotnet run --project ./VirtualFinland.UsersAPI.AdminFunction.CLI invalidate-caches`
