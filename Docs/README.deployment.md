# Deployment

## Fresh deployment

Deployment to a fresh environment requires the following steps to be performed in the deployment folder `VirtualFinland.UsersAPI.Deployment`:

- deploy the users-api infrastructure using pulumi:
  - Select the correct AWS profile to use for deployment:
    - `export AWS_PROFILE=<profile-name>`
  - Select the correct pulumi stack to use for deployment:
    - `pulumi stack select <stack-name>`
  - deploy the users-api with the selected AWS profile and pulumi stack:
    - `pulumi up`
- manually run the initial database migrations:
  - with a oneliner (with the correct AWS Profile):
    - `aws lambda invoke --payload '{"Action": "Migrate"}' --cli-binary-format raw-in-base64-out --function-name $(pulumi stack output AdminFunctionArn) output.json`
  - or by using the AWS Console:
    - Go to AWS Console and select Lambda
    - Select the function with name like `users-api-AdminFunctionArn-<stage>-<random-hash>`
    - Select the tab "Test"
    - Add a new test event with the following content: `{"Action": "Migrate"}`
    - Click "Test" button and wait for the execution to finish

## Deployment on existing environment
 
Pulumi does not have good tooling to handle attaching to existing resources such as AWS CloudWatch log groups. When deploying to a system where there are already resources created, one could tackle the problem of deployment failure by `ResourceAlreadyExistsException` by manually importing the resources to the pulumi state. 

Example for importing a CloudWatch log group of a RDS instance:
  - `pulumi import aws:cloudwatch/logGroup:LogGroup users-api-database-dev /aws/rds/instance/users-api-postgres-db-dev2313s/postgresql`

Read more of the pulumi import-tooling from [here](https://www.pulumi.com/docs/cli/commands/pulumi_import/).