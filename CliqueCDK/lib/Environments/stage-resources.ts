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
		dbName: "cliqueStage",

		engine: DatabaseClusterEngine.auroraMysql({
			version: AuroraMysqlEngineVersion.VER_3_04_0,
		}),
		instanceType: new ec2.InstanceType("t3.medium"),
		subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS,
		deleteAutomatedBackups: false,
		backup: {
			retention: Duration.days(3),
		},
	},
	redisCache: {
		numCacheNodes: 1,
	},
	customerLambdaProps: {
		firebaseProjectName: "cl1que-staging",
		dockerFileDir: "Dockerfile.Replication",
		memorySize: 1024,
		dataTracing: true,
	},
	runnerLambdaProps: {
		firebaseProjectName: "cl1que-picking-staging",
		dockerFileDir: "Dockerfile.Runner",
		memorySize: 1024,
		dataTracing: true,
	},
};
