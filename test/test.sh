#!/bin/bash

BRANCH="$(git branch | grep \* | cut -d ' ' -f2)"
NAME="armabot"
COMMIT="$(git log -1 --format=%h)"
TAG="${NAME}:${BRANCH}-${COMMIT}"

#Build

RES="$(./build.sh $TAG)"
ERROR=$?

if (( $ERROR > 0 )); then
	echo "Build Error"
	exit 1
fi


echo "Build of "$RES
