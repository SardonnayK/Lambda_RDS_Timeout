#!/usr/bin/env bash
if [[ $# -ge 1 ]]; then
    export DEPLOY_REGION=$1
    shift
    npx cdk deploy "$@" --require-approval never
    exit $?
else
    echo 1>&2 "Provide account and region as first two args."
    echo 1>&2 "Additional args are passed through to cdk deploy."
    exit 1
fi