#!/bin/sh

# This will download the mediapipe plugin files from github
FILE=com.github.homuler.mediapipe-0.10.1.tgz
SOURCE=package/Runtime/Plugins/
TARGET=../Packages/com.github.homuler.mediapipe/Runtime/

# Create temp folder
mkdir .curl
cd .curl

# Download the file
curl -L https://github.com/Kariaro/MediaPipeUnityPlugin/releases/download/v0.10.1/$FILE --output $FILE

# Unpack the file
tar zxvf $FILE

# Move the downloaded files
cp -r $SOURCE $TARGET

# Delete the temp folder
cd ..
rm -rf .curl
