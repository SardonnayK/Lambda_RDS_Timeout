import { AuroraMysqlEngineVersion, AuroraPostgresEngineVersion } from "aws-cdk-lib/aws-rds";
import { DatabaseClusterEngine } from "aws-cdk-lib/aws-rds";
import { EnvironmentOptions } from "./resources";
import * as ec2 from "aws-cdk-lib/aws-ec2";

export const environmentMapping: EnvironmentOptions = {
	vpc: {
		maxAZs: 2,
	},
	databaseProps: {
		dbName: "cliqueDev",
		engine: DatabaseClusterEngine.auroraPostgres({
			version: AuroraPostgresEngineVersion.VER_14_7,
		}),
		instanceType: new ec2.InstanceType("t3.medium"),
		subnetType: ec2.SubnetType.PUBLIC,
		replicaInstances: 1,
	},
	redisCache: {
		numCacheNodes: 1,
	},
	customerLambdaProps: {
		firebaseProjectName: "cl1que-dev",
		dockerFileDir: "Dockerfile",
		memorySize: 512,
		dataTracing: true,
	},
	runnerLambdaProps: {
		firebaseProjectName: "cl1que-picking-app",
		dockerFileDir: "Dockerfile.Runner",
		memorySize: 512,
		dataTracing: true,
	},
};
