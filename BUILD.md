# VRigUnity

## Setup

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
```

## Systems

### Windows
When building for windows you need to install **Inno Setup** and compile into the `Build/StandaloneWindows` folder

### Linux
Build into the folder `Build/StandaloneLinux` and zip the project
