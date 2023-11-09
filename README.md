## Prerequisites

This code requires .NET 6, Docker, and AWS CDK to be installed.
You can download .NET 6 from https://dotnet.microsoft.com/download/dotnet/6.0
You can download Docker from https://www.docker.com/get-started
You can download AWS CDK from https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html


## Update the .env

We first need to create a `.env` file inside the `CliqueCDK` directory. Copy the values from the `.dev.env` file
to pass all the required checks.


## Synthesize the App

To synthesize the app we need to execute the `synthesize-cdk-stack.sh` shell scrip by running the command
```
sh synthesize-cdk-stack.sh
```

## Deploy the App

To deploy the app we need to execute the `deploy-cdk-stack.sh` shell scrip by running the command
```
sh deploy-cdk-stack.sh
```
The app will be deployed to the 


# ALTERNATIVELY

If you wish to deploy to different regions please have a look at the cdk-deploy scripts within the `CliqueCDK` Folder.

## Publish the Application Code

To publish the API run the following commands:

```
dotnet restore

dotnet publish "RDS_Error_Repication/RDS_Error_Repication.csproj" --configuration Release --runtime linux-x64 --self-contained false --output ./app/RDS_Error_Repication/publish -p:GenerateRuntimeConfigurationFiles=true
```

## CDK Deployment

- `cd CliqueCDK` to navigate to the correct dir
- Run `npm i` to install the required node packages
- If the cdk has not been bootstrapped, run `cdk bootstrap`. Ensure the CDK is bootstrapped to the correct AWS region.

There is a couple of scripts available to handle deployment. The issue is most prevalent on the af-south-1 region.
First we need to ensure that the correct environment variables are present in `.env`. All the values can be random values as they just need to pass the cdk synth checks.
To deploy the code via the cdk run the command:

PLEASE NOTE: the deployment scripts are non-interactive. If you need to review what is deployed, review the script manually. 

The deployment configuration can be found under `CliqueCDK/lib/Environments`. The file is chosen based on the `ENVIRONMENT_NAME`
specified in `.env`

Run the command `sh cdk-af-south-1-deploy.sh` to deploy to south africa. The app needs to be published before we can deploy 
and the cdk needs to be bootstrapped in the correct environments. 
