#!/bin/sh
# This will download the mediapipe plugin files from github

if [ "$1" == "" ]; then
	# Default tag
	TAG=v0.10.1-macos11
else
	# Custom tag
	TAG=$1
fi

FILE=com.github.homuler.mediapipe-${TAG:1}.tgz
SOURCE=package/Runtime/Plugins/
TARGET=../Packages/com.github.homuler.mediapipe/Runtime/

# Create temp folder
mkdir .curl
cd .curl

# Download the file
STATUS=$(curl --write-out '%{http_code}' -L https://github.com/Kariaro/MediaPipeUnityPlugin/releases/download/$TAG/$FILE --output $FILE)

if [ "$STATUS" != "200" ]; then
	echo "Failed to download tag '${TAG}' from github. Status '${STATUS}'"
	cd ..
	rm -rf .curl
	exit
fi

# Unpack the file
tar zxvf $FILE

# Move the downloaded files
cp -r $SOURCE $TARGET

# Delete the temp folder
cd ..
rm -rf .curl
echo "Sucessfully downloaded and applied tag '${TAG}'"
