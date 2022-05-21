@echo off

:: BatchGotAdmin
:-------------------------------------
REM  --> Check for permissions
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"

REM --> If error flag set, we do not have admin.
if '%errorlevel%' NEQ '0' (
    echo Requesting administrative privileges...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    set params = %*:"=""
    echo UAC.ShellExecute "cmd.exe", "/c %~s0 %params%", "", "runas", 1 >> "%temp%\getadmin.vbs"

    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    pushd "%CD%"
    CD /D "%~dp0"
	set "UCCAPNAME=VRigUnity Video Capture"
    echo Installing capture device named '%UCCAPNAME%' ...
    regsvr32 "UnityCaptureFilter32bit.dll" "/i:UnityCaptureName=%UCCAPNAME%"
    regsvr32 "UnityCaptureFilter64bit.dll" "/i:UnityCaptureName=%UCCAPNAME%"
    echo "Done"
:--------------------------------------
