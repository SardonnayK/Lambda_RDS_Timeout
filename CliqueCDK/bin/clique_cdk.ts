#!/usr/bin/env node
import * as cdk from "aws-cdk-lib";
import { CliqueCdkStack } from "../lib/clique_cdk-stack";
import { EnvironmentOptions } from "../lib/Environments/resources";

const app = new cdk.App();

require("dotenv").config();
const deploymentEnvironment = process.env.ENVIRONMENT_NAME;

let environmentValue: EnvironmentOptions = require(`../lib/Environments/${deploymentEnvironment}-resources`).environmentMapping;
new CliqueCdkStack(app, `${deploymentEnvironment}-CliqueStack`, {
	deploymentEnvironment,
	environmentValue,
	env: {
		region: process.env.DEPLOY_REGION || process.env.CDK_DEFAULT_REGION,
		account: process.env.DEPLOY_ACCOUNT || process.env.CDK_DEFAULT_ACCOUNT,
	},
});
