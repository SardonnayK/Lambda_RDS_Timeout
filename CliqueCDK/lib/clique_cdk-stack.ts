import { DatabaseCluster } from "aws-cdk-lib/aws-rds";
import { Stack, StackProps, App, aws_ec2, CfnParameter } from "aws-cdk-lib";
import { Function, IFunction } from "aws-cdk-lib/aws-lambda";
import { Construct } from "constructs";
import { CacheRefresher } from "./CacheRefresher";
import { CliqueDB } from "./CliqueDatabase";
import { CliqueRedisCache } from "./CliqueRedisCache";
import { EnvironmentOptions } from "./Environments/resources";
import { LambdaAPI } from "./lambdaApi";
import { v4 as uuidv4 } from "uuid";
import { Topic } from "aws-cdk-lib/aws-sns";
import { QueueConsumer } from "./QueueConsumer";
import { SqsSubscription } from "aws-cdk-lib/aws-sns-subscriptions";

export interface CliqueStackProps extends StackProps {
  environmentValue: EnvironmentOptions;
  deploymentEnvironment?: string;
}

export class CliqueCdkStack extends Stack {
  constructor(scope: Construct, id: string, props: CliqueStackProps) {
    super(scope, id, props);

    const environmentValue = props.environmentValue;

    require("dotenv").config();
    const deploymentEnvironment = props.deploymentEnvironment;
    const STOREFRONT_URI = process.env.STOREFRONT_URI;
    const STOREFRONT_KEY = process.env.STOREFRONT_KEY;
    const SHOPIFY_URL = process.env.SHOPIFY_URL;
    const SHOP_ACCESS_TOKEN = process.env.SHOP_ACCESS_TOKEN;

    const WP_URL = process.env.WP_URL;
    const WP_CLIENT_ID = process.env.WP_CLIENT_ID;
    const WP_CLIENT_SECRET = process.env.WP_CLIENT_SECRET;
    const WP_SCOPE = process.env.WP_SCOPE;
    const WOLFPACK_SHARED_SECRET = process.env.WOLFPACK_SHARED_SECRET;

    const FIREBASE_KEY = process.env.FIREBASE_KEY;
    const RUNNER_FIREBASE_KEY = process.env.RUNNER_FIREBASE_KEY;

    const FRESH_DESK_API_KEY = process.env.FRESH_DESK_API_KEY;
    const SHOPIFY_WEBHOOK_SECRET = process.env.SHOPIFY_WEBHOOK_SECRET;

    const Cache_Validation_Key = uuidv4();

    if (!deploymentEnvironment) {
      throw new Error("No Environment Name Provided");
    }

    if (!SHOPIFY_WEBHOOK_SECRET) {
      throw new Error("Missing Shopify webhook");
    }
    if (
      !STOREFRONT_URI ||
      !STOREFRONT_KEY ||
      !SHOPIFY_URL ||
      !SHOP_ACCESS_TOKEN
    ) {
      throw new Error("Missing or Insufficient Shopify Credentials");
    }

    if (
      !WP_URL ||
      !WP_CLIENT_ID ||
      !WP_CLIENT_SECRET ||
      !WP_SCOPE ||
      !WOLFPACK_SHARED_SECRET
    ) {
      throw new Error("Missing or Insufficient Wolfpack Credentials");
    }

    if (!FIREBASE_KEY || !RUNNER_FIREBASE_KEY) {
      throw new Error("Missing or Insufficient Firebase settings");
    }
    if (!FRESH_DESK_API_KEY) {
      throw new Error("Missing or Insufficient Freshdesk Credentials");
    }

    const vpc = new aws_ec2.Vpc(this, `${deploymentEnvironment}-Vpc`, {
      subnetConfiguration: [
        {
          name: "public",
          subnetType: aws_ec2.SubnetType.PUBLIC,
        },
        {
          name: "private",
          subnetType: aws_ec2.SubnetType.PRIVATE_WITH_EGRESS,
        },
        {
          name: "isolated",
          subnetType: aws_ec2.SubnetType.PRIVATE_ISOLATED,
        },
      ],
      maxAzs: props.environmentValue.vpc.maxAZs,
    });

    const db = new CliqueDB(this, `${deploymentEnvironment}-CliqueDB`, {
      vpc,
      environmentName: deploymentEnvironment,
      ...environmentValue.databaseProps,
    });

    const redis = new CliqueRedisCache(
      this,
      `${deploymentEnvironment}-CliqueRedis`,
      {
        vpc,
        environmentName: deploymentEnvironment,
        ...environmentValue.redisCache,
      }
    );

    const customerAPI = new LambdaAPI(
      this,
      `${deploymentEnvironment}-LambdaAPI`,
      {
        vpc,
        db: db.instance,
        redis: redis.instance,
        ...environmentValue.customerLambdaProps,
      }
    );

    customerAPI._handler.connections.allowTo(
      db.instance,
      aws_ec2.Port.allTcp(),
      "Access to DB From CustomerAPI"
    );

    db.proxy.grantConnect(customerAPI._handler);

    customerAPI._handler.connections.allowTo(
      redis.redis_sg,
      aws_ec2.Port.allTcp(),
      "Access to the redis cache From CustomerAPI"
    );

    const orderTopic = new Topic(this, "order-topic");
    const customerTopic = new Topic(this, "customer-topic");

    // TODO: Add the environeent variables for the WOLFPACK_SHARED_SECRET

    var environmentVariables = this.CreateEnvironmentVariables(
      STOREFRONT_URI,
      STOREFRONT_KEY,
      SHOPIFY_URL,
      SHOP_ACCESS_TOKEN,
      WP_URL,
      WP_CLIENT_ID,
      WP_CLIENT_SECRET,
      WP_SCOPE,
      WOLFPACK_SHARED_SECRET,
      FIREBASE_KEY,
      db.databaseName,
      FRESH_DESK_API_KEY,
      db.proxy.endpoint,
      db.instance,
      environmentValue.customerLambdaProps.firebaseProjectName,
      redis?.instance?.attrRedisEndpointAddress ||
        redis?.instance?.attrConfigurationEndpointAddress,
      Cache_Validation_Key,
      orderTopic.topicArn,
      customerTopic.topicArn,
      SHOPIFY_WEBHOOK_SECRET
    );

    this.AddEnvironmentVariables(environmentVariables, customerAPI._handler);

    environmentVariables.FIREBASE_KEY = RUNNER_FIREBASE_KEY;
    environmentVariables.FIREBASE_PROJECT_NAME =
      environmentValue.runnerLambdaProps.firebaseProjectName;
    // this.AddEnvironmentVariables(environmentVariables, runnerAPI._handler);

    orderTopic.grantPublish(customerAPI._handler);
    customerTopic.grantPublish(customerAPI._handler);
  }

  private CreateEnvironmentVariables(
    STOREFRONT_URI: string,
    STOREFRONT_KEY: string,
    SHOPIFY_URL: string,
    SHOP_ACCESS_TOKEN: string,
    WP_URL: string,
    WP_CLIENT_ID: string,
    WP_CLIENT_SECRET: string,
    WP_SCOPE: string,
    WOLFPACK_SHARED_SECRET: string,
    FIREBASE_KEY: string,
    DATABASE_NAME: string,
    FRESH_DESK_API_KEY: string,
    DB_HOSTNAME: string,
    db: DatabaseCluster,
    FIREBASE_PROJECT_NAME: string,
    REDIS_ENDPOINT: string,
    CacheValidationKey: string,
    ORDER_TOPIC_ARN: string,
    CUSTOMER_TOPIC_ARN: string,
    SHOPIFY_WEBHOOK_SECRET: string
  ) {
    const environmentVariables = {
      STOREFRONT_URI,
      STOREFRONT_KEY,
      SHOPIFY_URL,
      SHOP_ACCESS_TOKEN,
      WP_URL,
      WP_CLIENT_ID,
      WP_CLIENT_SECRET,
      WP_SCOPE,
      WOLFPACK_SHARED_SECRET,
      FIREBASE_KEY,
      DATABASE_NAME,
      FRESH_DESK_API_KEY,

      DB_HOSTNAME,
      SECRET_ARN: db.secret?.secretArn || "",
      SECRET_NAME: db.secret?.secretName || "",
      FIREBASE_PROJECT_NAME: FIREBASE_PROJECT_NAME || "cl1que-dev",
      REDIS_ENDPOINT,
      CacheValidationKey,
      ORDER_TOPIC_ARN,
      CUSTOMER_TOPIC_ARN,
      SHOPIFY_WEBHOOK_SECRET,
    };

    return environmentVariables;
  }

  private AddEnvironmentVariables(environmentVariables: any, api: Function) {
    for (const key in environmentVariables) {
      if (Object.prototype.hasOwnProperty.call(environmentVariables, key)) {
        const value = environmentVariables[key];
        api.addEnvironment(key, value);
      }
    }
  }
}
function createCustomerQueue(scope: Construct): QueueConsumer {
  throw new Error("Function not implemented.");
}
function createOrderQueue(): QueueConsumer {
  throw new Error("Function not implemented.");
}
