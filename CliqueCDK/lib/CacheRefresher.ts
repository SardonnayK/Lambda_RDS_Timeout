import { Port, SubnetType, Vpc } from "aws-cdk-lib/aws-ec2";
import { CfnOutput, Duration } from "aws-cdk-lib";
import * as lambda from "aws-cdk-lib/aws-lambda";
import { Construct } from "constructs";
import * as iam from "aws-cdk-lib/aws-iam";
import * as events from "aws-cdk-lib/aws-events";
import * as targets from "aws-cdk-lib/aws-events-targets";
import * as path from "path";
import { RetentionDays } from "aws-cdk-lib/aws-logs";

export interface CacheRefresherProps {
	vpc: Vpc;
	CacheValidationKey: string;
}

export class CacheRefresher extends Construct {
	public _handler: lambda.Function;
	constructor(scope: Construct, id: string, props: CacheRefresherProps) {
		super(scope, id);
		const directory = path.resolve(__dirname, "../../");

		// #region API that will handle the cache update
		const dockerImageCode = lambda.DockerImageCode.fromImageAsset(directory, { file: "Dockerfile.cache-refresher" });

		const myRole = new iam.Role(this, "Cache-Refresher-role", {
			assumedBy: new iam.ServicePrincipal("lambda.amazonaws.com"),
		});

		const api = new lambda.DockerImageFunction(this, "cache-refresh-api", {
			code: dockerImageCode,
			tracing: lambda.Tracing.ACTIVE,
			timeout: Duration.seconds(30),
			vpc: props.vpc,
			vpcSubnets: { subnetType: SubnetType.PRIVATE_WITH_EGRESS },
			logRetention: RetentionDays.TWO_WEEKS,
		});

		const apiUrl = api.addFunctionUrl({
			authType: lambda.FunctionUrlAuthType.NONE,
		});

		apiUrl.grantInvokeUrl(myRole);

		myRole.addManagedPolicy(iam.ManagedPolicy.fromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"));
		myRole.addManagedPolicy(iam.ManagedPolicy.fromAwsManagedPolicyName("service-role/AWSLambdaVPCAccessExecutionRole"));

		new CfnOutput(this, "cache_api_Url", {
			// The .url attributes will return the unique Function URL
			value: apiUrl.url,
		});
		this._handler = api;
		//#endregion

		const storeLambda = new lambda.Function(this, "store-cache-refresher", {
			code: lambda.Code.fromAsset("./lib/functions/cache_update_stores"),
			runtime: lambda.Runtime.PYTHON_3_9,
			handler: "cache_update_stores.lambda_handler",
			tracing: lambda.Tracing.ACTIVE,
			vpc: props.vpc,
		});

		const mallLambda = new lambda.Function(this, "mall-cache-refresher", {
			code: lambda.Code.fromAsset("./lib/functions/cache_update_malls"),
			runtime: lambda.Runtime.PYTHON_3_9,
			handler: "cache_update_malls.lambda_handler",
			tracing: lambda.Tracing.ACTIVE,
			vpc: props.vpc,
		});

		const categoryLambda = new lambda.Function(this, "category-cache-refresher", {
			code: lambda.Code.fromAsset("./lib/functions/cache_update_categories"),
			runtime: lambda.Runtime.PYTHON_3_9,
			handler: "cache_update_categories.lambda_handler",
			tracing: lambda.Tracing.ACTIVE,
			vpc: props.vpc,
		});

		mallLambda.addEnvironment("site", apiUrl.url);
		storeLambda.addEnvironment("site", apiUrl.url);
		categoryLambda.addEnvironment("site", apiUrl.url);

		mallLambda.addEnvironment("CacheValidationKey", props.CacheValidationKey);
		storeLambda.addEnvironment("CacheValidationKey", props.CacheValidationKey);
		categoryLambda.addEnvironment("CacheValidationKey", props.CacheValidationKey);

		const storeEvent = new events.Rule(this, "update-stpre-cache-rule", {
			schedule: events.Schedule.cron({ weekDay: "1", hour: "1", minute: "0" }),
		});
		const eventRule = new events.Rule(this, "update-all-cache-rule", {
			schedule: events.Schedule.cron({ weekDay: "1", hour: "3", minute: "0" }),
		});
		eventRule.addTarget(new targets.LambdaFunction(mallLambda));
		storeEvent.addTarget(new targets.LambdaFunction(storeLambda));
		eventRule.addTarget(new targets.LambdaFunction(categoryLambda));

		apiUrl.grantInvokeUrl(mallLambda.role!);
		apiUrl.grantInvokeUrl(storeLambda.role!);
		apiUrl.grantInvokeUrl(categoryLambda.role!);

		mallLambda.connections.allowTo(api, Port.allTcp());
		storeLambda.connections.allowTo(api, Port.allTcp());
		categoryLambda.connections.allowTo(api, Port.allTcp());
	}
}
