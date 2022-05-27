#!/bin/bash

if [ $(id -u) != 0 ]; then
    echo "This Install script requires root permissions"
    sudo "$0" "$@"
    exit
fi

# TODO: Ask the user if v4l2loopback is installed

# Register virtual camera device
modprobe v4l2loopback \
    exclusive_caps=1 \
    card_label="VRigUnity Video Capture"
