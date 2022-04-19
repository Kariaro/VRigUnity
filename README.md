# VRigUnity
Test rig unity project.


## Dependencies
[MediaPipeUnityPlugin v0.9.1](https://github.com/homuler/MediaPipeUnityPlugin)
+ [OpenCV 3.4.16](https://opencv.org/releases/)

[UniVRM v0.97.0](https://github.com/vrm-c/UniVRM)

## Setup

### UniVRM
Install the version `v0.97.0` and navigate `Assets > Import Package > Custom Package...` and add the `.unitypackage` file.

### MediaPipeUnityPlugin
Go to the github page and follow the build instructions.

Use `git-bash` when compiling the code.
Make sure you have installed the exact version `OpenCV 3.4.16` and `Python 3.10`
To remove some windows errors add this command before you build.
```shell
export MSYS2_ARG_CONV_EXCL="*"
```

Scrap notes I wrote when I got `MediaPipeUnityPlugin` working.

```
# C:\Program Files\Python310

# Must use git-bash for 'cp' command

# Must type in git-bash
export MSYS2_ARG_CONV_EXCL="*"

# Must use
OpenCV – 3.4.16
```