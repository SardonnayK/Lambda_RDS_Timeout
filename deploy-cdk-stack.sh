dotnet restore
dotnet publish "RDS_Error_Repication/RDS_Error_Repication.csproj" --configuration Release --runtime linux-x64 --self-contained false --output ./app/RDS_Error_Repication/publish -p:GenerateRuntimeConfigurationFiles=true

cd CliqueCDK
sh cdk-af-south-1-deploy.sh