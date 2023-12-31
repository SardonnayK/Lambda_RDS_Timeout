import { Construct } from "constructs";
import { Stack, StackProps, App, Duration, CfnOutput } from "aws-cdk-lib";
import * as aws_rds from "aws-cdk-lib/aws-rds";
import * as ec2 from "aws-cdk-lib/aws-ec2";

export class DatabaseProps {
	// Properties used to alter the database.
	vpc: ec2.Vpc;
	instanceType?: ec2.InstanceType;
	engine: aws_rds.IClusterEngine;
	dbName: string;
	publiclyAccessible?: boolean = true;

	// =================================
	subnetType?: ec2.SubnetType;
	deleteAutomatedBackups?: boolean;
	backup?: aws_rds.BackupProps;
	replicaInstances?: number;
	environmentName: string;
}

export class CliqueDB extends Construct {
	readonly instance: aws_rds.DatabaseCluster;
	readonly proxy: aws_rds.IDatabaseProxy;
	readonly databaseName: string;

	constructor(scope: Construct, id: string, props: DatabaseProps) {
		super(scope, id);

		// #region setting defaults
		props.publiclyAccessible = props.publiclyAccessible ?? true;
		props.instanceType = props.instanceType ?? new ec2.InstanceType("t2.small");
		props.subnetType = props.subnetType ?? ec2.SubnetType.PUBLIC;
		props.deleteAutomatedBackups = props.deleteAutomatedBackups ?? true;
		//#endregion

		const engine = props.engine;
		const instance = new aws_rds.DatabaseCluster(this, `${props.environmentName}-Database`, {
			engine,
			defaultDatabaseName: props.dbName,
			vpc: props?.vpc,
			vpcSubnets: { subnetType: props.subnetType },
			writer: aws_rds.ClusterInstance.provisioned("writer", {
				instanceType: props.instanceType,
				publiclyAccessible: true,
				enablePerformanceInsights: true,
			}),
			readers: [
				aws_rds.ClusterInstance.provisioned("reader", {
					promotionTier: 1,
				}),
			],
			backup: props.backup,
			preferredMaintenanceWindow: "Mon:03:00-Mon:03:30",
		});

		this.instance = instance;
		this.databaseName = props.dbName;

		//--------------DATABASE PROXY--------------------------

		this.proxy = new aws_rds.DatabaseProxy(this, `${props.environmentName}Proxy`, {
			proxyTarget: aws_rds.ProxyTarget.fromCluster(instance),
			secrets: [instance.secret!],
			vpc: props.vpc,
			iamAuth: false,
			requireTLS: false,
			maxConnectionsPercent: 80,
			securityGroups: instance.connections.securityGroups,
			vpcSubnets: { subnetType: props.subnetType },
			idleClientTimeout: Duration.minutes(20),
		});
		new CfnOutput(this, `${props.environmentName}-proxy-endpoint`, {
			exportName: `${props.environmentName}-proxy-endpoint`,
			value: this.proxy.endpoint,
		});
	}
}
