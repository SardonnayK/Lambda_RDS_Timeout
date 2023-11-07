import { Construct } from "constructs";
import { aws_ecr_assets, aws_iam, Duration } from "aws-cdk-lib";
import { Alias, DockerImageCode, DockerImageFunction, Runtime, Tracing, Version } from "aws-cdk-lib/aws-lambda";
import { Cors, CorsOptions, LambdaRestApi, LambdaRestApiProps, MethodLoggingLevel } from "aws-cdk-lib/aws-apigateway";
import * as path from "path";
import { Vpc } from "aws-cdk-lib/aws-ec2";
import { DatabaseCluster } from "aws-cdk-lib/aws-rds";
import { CfnCacheCluster } from "aws-cdk-lib/aws-elasticache";
import { RetentionDays } from "aws-cdk-lib/aws-logs";

export interface LambdaApiProps {
	vpc: Vpc;
	db: DatabaseCluster;
	firebaseProjectName: string;
	dockerFileDir: string;
	redis: CfnCacheCluster;
	memorySize?: 256 | 512 | 1024 | 2048 | 4096;
	maxProvisionedConcurrency?: 1 | 2 | 3 | 4 | 5 | 7 | 10;
	dataTracing: boolean;
}

export class LambdaAPI extends Construct {
	readonly _handler: DockerImageFunction;

	constructor(scope: Construct, id: string, props: LambdaApiProps) {
		super(scope, id);

		const lambdaRole = new aws_iam.Role(this, "lambdaRole", { assumedBy: new aws_iam.ServicePrincipal("lambda.amazonaws.com") });
		lambdaRole.addManagedPolicy(aws_iam.ManagedPolicy.fromAwsManagedPolicyName("AdministratorAccess"));

		const directory = path.resolve(__dirname, "../../");

		const dockerAssetCode = DockerImageCode.fromImageAsset(directory, {
			file: props.dockerFileDir,
		});

		const handler = new DockerImageFunction(this, "Handler", {
			code: dockerAssetCode,
			timeout: Duration.minutes(3),
			vpc: props.vpc,
			role: lambdaRole,
			logRetention: RetentionDays.ONE_MONTH,
			reservedConcurrentExecutions: 100,
			memorySize: props.memorySize,
			tracing: Tracing.PASS_THROUGH,
			environment: {
				DB_HOSTNAME: props.db.clusterEndpoint.hostname,
				SECRET_ARN: props.db.secret?.secretArn || "",
				SECRET_NAME: props.db.secret?.secretName || "",
				FIREBASE_PROJECT_NAME: props.firebaseProjectName || "cl1que-dev",
				REDIS_ENDPOINT: props.redis?.attrRedisEndpointAddress || props.redis?.attrConfigurationEndpointAddress,
			},
		});

		this._handler = handler;
		const corsOptions: CorsOptions = {
			allowHeaders: ["Content-Type", "X-Amz-Date", "Authorization", "X-Api-Key"],
			allowMethods: ["OPTIONS", "GET", "POST", "PUT", "PATCH", "DELETE"],
			allowOrigins: Cors.ALL_ORIGINS,
			allowCredentials: false,
		};
		const api = new LambdaRestApi(this, "lambdaApi", {
			handler,
			defaultCorsPreflightOptions: corsOptions,
			deployOptions: {
				tracingEnabled: true,
				metricsEnabled: true,
				loggingLevel: MethodLoggingLevel.ERROR,
				dataTraceEnabled: props.dataTracing,
			},
		});

		if (props.maxProvisionedConcurrency) {
			const version = this._handler.currentVersion;

			const alias = new Alias(this, `${props.firebaseProjectName}-Alias`, {
				aliasName: "prod",
				provisionedConcurrentExecutions: props.maxProvisionedConcurrency,

				version,
			});

			const autoScaler = alias.addAutoScaling({ maxCapacity: props.maxProvisionedConcurrency });
			autoScaler.scaleOnUtilization({
				utilizationTarget: 0.75,
				scaleInCooldown: Duration.seconds(10),
				disableScaleIn: false,
			});
			autoScaler.node.addDependency(this._handler);
		}
	}
}
