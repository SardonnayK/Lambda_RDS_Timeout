## Prerequisites

This code requires .NET 6, Docker, and AWS CDK to be installed.
You can download .NET 6 from https://dotnet.microsoft.com/download/dotnet/6.0
You can download Docker from https://www.docker.com/get-started
You can download AWS CDK from https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html


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
First we need to ensure that the correct environment variables are present in `.env`
To deploy the code via the cdk run the command:

PLEASE NOTE: the deployment scripts are non-interactive. If you need to review what is deployed, review the script manually. 

Run the command `sh cdk-af-south-1-deploy.sh` to deploy to south africa. The app needs to be published before we can deploy 
and the cdk needs to be bootstrapped in the correct environments. 
