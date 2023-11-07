import { Construct } from "constructs";
import { aws_ecr_assets, aws_iam, Duration } from "aws-cdk-lib";
import { Alias, DockerImageCode, DockerImageFunction, FunctionUrlAuthType, Runtime, Tracing, Version } from "aws-cdk-lib/aws-lambda";
import { Cors, CorsOptions, LambdaRestApi, LambdaRestApiProps, MethodLoggingLevel } from "aws-cdk-lib/aws-apigateway";
import * as path from "path";
import { Vpc } from "aws-cdk-lib/aws-ec2";
import { DatabaseCluster } from "aws-cdk-lib/aws-rds";
import { CfnCacheCluster } from "aws-cdk-lib/aws-elasticache";
import { RetentionDays } from "aws-cdk-lib/aws-logs";
import { IQueue, Queue } from "aws-cdk-lib/aws-sqs";
import { ManagedPolicy, Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { SqsEventSource } from "aws-cdk-lib/aws-lambda-event-sources";

export interface LambdaApiProps {
	consumerName: string;
	dockerImageFile: string;
	dockerImagePath: string;
}

export class QueueConsumer extends Construct {
	readonly function: DockerImageFunction;
	readonly queue: IQueue;

	constructor(scope: Construct, id: string, props: LambdaApiProps) {
		super(scope, id);

		const queue = new Queue(this, `${props.consumerName}-queue`, {
			visibilityTimeout: Duration.seconds(100),
			receiveMessageWaitTime: Duration.seconds(10),
		});
		const queueArn = queue.queueArn;

		// #region Consumer lambda
		const dockerDirectory = path.resolve(__dirname, props.dockerImagePath);
		const consumerDockerImageCode = DockerImageCode.fromImageAsset(dockerDirectory, {
			file: props.dockerImageFile,
		});

		const consumerRole = new Role(this, `${props.consumerName}-role`, {
			assumedBy: new ServicePrincipal("lambda.amazonaws.com"),
		});

		this.function = new DockerImageFunction(this, `${props.consumerName}-api`, {
			code: consumerDockerImageCode,
			tracing: Tracing.ACTIVE,
			timeout: Duration.seconds(30),
			logRetention: RetentionDays.TWO_WEEKS,
			environment: {
				SQS_ARN: queueArn,
				SQS_NAME: queue.queueName,
			},
		});

		const consumerUrl = this.function.addFunctionUrl({
			authType: FunctionUrlAuthType.NONE,
		});

		consumerUrl.grantInvokeUrl(consumerRole);

		consumerRole.addManagedPolicy(ManagedPolicy.fromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"));

		consumerRole.addManagedPolicy(ManagedPolicy.fromAwsManagedPolicyName("service-role/AWSLambdaVPCAccessExecutionRole"));

		// #region Add an event for the lambda trigger
		const queueEventSource = new SqsEventSource(queue, {
			batchSize: 10,
			reportBatchItemFailures: true,
		});
		this.function.addEventSource(queueEventSource);
		queue.grantConsumeMessages(this.function);
		// #endregion
		this.queue = queue;
	}
}
