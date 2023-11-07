import { DatabaseProps } from "../CliqueDatabase";
import { CliqueRedisCacheProps } from "../CliqueRedisCache";
import { LambdaApiProps } from "../lambdaApi";

export interface EnvironmentOptions {
	vpc: { maxAZs: number };
	databaseProps: Omit<DatabaseProps, "vpc" | "environmentName">;
	redisCache: Omit<CliqueRedisCacheProps, "vpc" | "environmentName">;
	customerLambdaProps: Omit<LambdaApiProps, "vpc" | "db" | "redis">;
	runnerLambdaProps: Omit<LambdaApiProps, "vpc" | "db" | "redis">;
}

export class EnvironmentMapping {
	[key: string]: EnvironmentOptions;
}
