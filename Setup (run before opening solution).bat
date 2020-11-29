:: This script creates a symlink to the torch and game binaries to account for different installation directories on different systems.
@echo off

echo #############################################################
echo ##                                                         ##
echo ##   Example folder location: C:\Torch Servers\My Server   ##
echo ##                                                         ##
echo #############################################################
set /p path="Please enter the folder location of your Torch.Server.exe: "
echo path = %path%

cd %~dp0
echo cd = %cd%

if exist TorchBinaries (
    rmdir TorchBinaries
    echo TorchBinaries old symlink deleted
)

mklink /J TorchBinaries "%path%"
if errorlevel 1 (
    goto ErrorTorchBinaries
)

if exist GameBinaries (
    rmdir GameBinaries
    echo GameBinaries old symlink deleted
)

mklink /J GameBinaries "%path%\DedicatedServer64"
if errorlevel 1 (
    goto ErrorGameBinaries
)


goto EndScript

:ErrorTorchBinaries
echo An error occured creating the TorchBinaries symlink.
goto EndScript
:ErrorGameBinaries
echo An error occured creating the GameBinaries symlink.
goto EndScript

:EndScript
echo All symlinks re-created successfully!
pause