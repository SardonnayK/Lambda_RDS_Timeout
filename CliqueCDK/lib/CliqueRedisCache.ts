import { Construct } from "constructs";
import * as ec2 from "aws-cdk-lib/aws-ec2";
import { CfnSubnetGroup, CfnCacheCluster } from "aws-cdk-lib/aws-elasticache";
import { SecurityGroup } from "aws-cdk-lib/aws-ec2";

export class CliqueRedisCacheProps {
	vpc: ec2.Vpc;
	numCacheNodes: number;
	environmentName: string;
	cacheNodeType?: string;
}

export class CliqueRedisCache extends Construct {
	readonly instance: CfnCacheCluster;
	readonly redis_sg: SecurityGroup;

	constructor(scope: Construct, id: string, props: CliqueRedisCacheProps) {
		super(scope, id);

		props.cacheNodeType = props.cacheNodeType ?? "cache.t3.micro";

		const redisSubnetGroup = new CfnSubnetGroup(this, `${props.environmentName}-RedisClusterIsolatedSubnetGroup`, {
			cacheSubnetGroupName: `${props.environmentName}-private-redis`,
			subnetIds: props.vpc.isolatedSubnets.map((isolatedSubnets) => isolatedSubnets.subnetId),
			description: `${props.environmentName} isolated subnet group.`,
		});

		const redisSecurityGroup = new SecurityGroup(this, `${props.environmentName}-RedisClusterSecurityGroup`, {
			vpc: props.vpc,
			securityGroupName: `${props.environmentName}-redis-sg`,
		});

		const redisReplication = new CfnCacheCluster(this, `${props.environmentName}-RedisReplicaGroup`, {
			engine: "redis",
			numCacheNodes: props.numCacheNodes,
			cacheNodeType: props.cacheNodeType,
			autoMinorVersionUpgrade: true,
			cacheSubnetGroupName: redisSubnetGroup.cacheSubnetGroupName,

			vpcSecurityGroupIds: [redisSecurityGroup.securityGroupId],
		});

		redisReplication.addDependency(redisSubnetGroup);
		this.instance = redisReplication;
		this.redis_sg = redisSecurityGroup;
	}
}
