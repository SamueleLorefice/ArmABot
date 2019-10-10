#!/bin/bash

## Tag Generation
TAG="$1"
if(($#<1));then
	BRANCH="$(git branch | grep \* | cut -d ' ' -f2)"
	NAME=""
	COMMIT="$(git log -1 --format=%h)"
	TAG="${NAME}:${BRANCH}-${COMMIT}"
fi

echo "Building "$TAG

IMAGE_ID="$(docker build ../ -q -t $TAG)"
ERROR="$?"

if (($ERROR>0)); then
	echo "Error: Docker Build: "$ERROR
	exit ${ERROR}
else
	echo "Image ID: $IMAGE_ID"
fi

exit 0
