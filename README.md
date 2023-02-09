# Users API

This project is intended to create user profiles and related user specific data based on outside identity providers.  
A user authentication information is checked against the API database to see if the use exists and then create an instance in the database.

# Development Environment

## Requirements

The project is created primarily on C# and .NET.

Install Amazon.Lambda.Tools Global Tools if not already installed.

```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.

```
    dotnet tool update -g Amazon.Lambda.Tools
```

Install .NET SDK.  
https://dotnet.microsoft.com/en-us/download

Install Pulumi.  
https://www.pulumi.com/docs/get-started/install/

Docker is used to support local development but not necessary.  
https://docs.docker.com/get-docker/

#### Docker requirements

If using docker compose, the following network must be created: `vfd-network`.

Create the network with the following command:

```
docker network create vfd-network
```

## Project Structure

1. **VirtualFinland.UsersAPI**: The API functionalities
2. **VirtualFinland.UsersAPI.Deployment**: The Pulumi IaC definitions for AWS resources provisioning
3. **VirtualFinland.UsersAPI.IntegrationTests**: Tests for the API functionalities
4. **Tools**: Contains scripts and definitions that help with the development process

## Q&A

### How do I start developing

Pick your choice of an IDE or similar developer tool that supports C# and .NET.

You can start by opening the solution file **VirtualFinland.UsersAPI.sln** or by opening the this root folder in a code editor like Visual Studio Code.

### What is the easiest way to test a client application with the API

Use premade scripts that build and start the API with a database using docker. You will need to have docker installed.

To start the API in docker use the following script x86 CPUs, call in the repository root.

```
    ./Tools/Scripts/startApiInDocker.sh
```

Or the following script or arm64 CPUs, call in the repository root.

```
    ./Tools/Scripts/startApiInDocker-arm64.sh
```

The API should be available at the following address:  
http//localhost:5001/

Notice: If you are running a Linux or MacOS operating system you might need to provide execution rights to the file, sample command:

```
    chmod 755 ./Tools/Scripts/startApiInDocker.sh
```

### What address does the application use in local development

Two addresses:

- Primary: https://localhost:5001/
- Secondary: http://localhost:5001/

### How to build AWS Lambda Deployment package

Navigate to the VirtualFinland.UsersAPI project root folder (./VirtualFinland.UserAPI/src/VirtualFinland.UsersAPI) and run the lambda tools package command.

```
  dotnet lambda package
```

### Which environments are supported

At the moment only a development/staging environment configurations are supported. You can set the following environment variable.

```
  ASPNETCORE_ENVIRONMENT=dev
```

### What functionalities and requests the API has

You can use the swagger UI when the API is started in development mode.
https://localhost:5001/swagger/

Or you can use premade Postman Collection with ready made environments to testing, these can be found in the **/Tools/Postman** folder.

### How to deploy the infrastructure into AWS

You need to have the Pulumi tools installed in your system and do some of the following things:

If new to Pulumi, then read start here: https://www.pulumi.com/docs/get-started/aws/

Other good to know documentation:  
https://www.pulumi.com/docs/intro/concepts/how-pulumi-works/  
https://www.pulumi.com/docs/intro/concepts/stack/  
https://www.pulumi.com/docs/intro/concepts/secrets/  
https://www.pulumi.com/docs/intro/concepts/config/  
https://www.pulumi.com/docs/intro/concepts/inputs-outputs/

#### Pulumi basic commands

- "pulumi preview": reads you stacks and generates a preview of to be provisioned resources
- "pulumi up": Same as preview but will start to preform the actual resources provisioning (create or update) after manual acceptance
- "pulumi destroy": Will destroy the provisioned resources
- "pulumi stack select **mystackname**": Will swap to a different stack
