import { AuroraMysqlEngineVersion, AuroraPostgresEngineVersion } from "aws-cdk-lib/aws-rds";
import { DatabaseClusterEngine } from "aws-cdk-lib/aws-rds";
import { EnvironmentOptions } from "./resources";
import * as ec2 from "aws-cdk-lib/aws-ec2";
import { Duration } from "aws-cdk-lib";

export const environmentMapping: EnvironmentOptions = {
	vpc: {
		maxAZs: 2,
	},
	databaseProps: {
		dbName: "cliqueProd",

		engine: DatabaseClusterEngine.auroraPostgres({
			version: AuroraPostgresEngineVersion.VER_14_7,
		}),
		instanceType: new ec2.InstanceType("t3.medium"),
		subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS,
		deleteAutomatedBackups: false,
		backup: {
			retention: Duration.days(7),
		},
	},
	redisCache: {
		numCacheNodes: 1,
	},
	customerLambdaProps: {
		firebaseProjectName: "cl1que-prod",
		dockerFileDir: "Dockerfile",
		memorySize: 4096,
		maxProvisionedConcurrency: 1,
		dataTracing: false,
	},
	runnerLambdaProps: {
		firebaseProjectName: "cl1que-picking-prod",
		dockerFileDir: "Dockerfile.Runner",
		memorySize: 1024,
		maxProvisionedConcurrency: 1,
		dataTracing: false,
	},
};
