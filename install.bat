@echo off

SET prjDir=%CD%\WhMgr
SET binDir=%prjDir%\bin\debug\netcoreapp2.1

:: Download .NET Core 2.1 installer
echo "Downloading .NET Core 2.1 installer..."
powershell -Command "iwr -outf ~/Desktop/dotnet-install.ps1 https://dotnet.microsoft.com/download/dotnet-core/scripts/v1/dotnet-install.ps1"

:: Make installer executable

:: Install .NET Core 2.1.0
echo "Launching .NET Core installer..."
powershell -ExecutionPolicy RemoteSigned -File dotnet-install.ps1 -Version 2.1.803

:: Delete .NET Core 2.1.0 installer
echo "Deleting .NET Core installer..."
del dotnet-install.ps1

:: Clone repository
echo "Cloning repository..."
git clone https://github.com/versx/WhMgr

:: Change directory into cloned repository
echo "Changing directory..."
cd %prjDir%

:: Build WhMgr.dll
echo "Building WhMgr..."
dotnet build

:: Copy example config
echo "Copying example files..."
xcopy /s /e %prjDir%\examples\Alerts\* %binDir%\Alerts\
xcopy /s /e %prjDir%\examples\Filters\* %binDir%\Filters\
xcopy /s /e %prjDir%\examples\Geofences\* %binDir%\Geofences\
xcopy /s /e %prjDir%\examples\Templates\* %binDir%\Templates\
xcopy /s /e %prjDir%\static\* %binDir%\static\
xcopy %prjDir%\alarms.example.json %binDir%\alarms.json*
xcopy %prjDir%\config.example.json %binDir%\config.json*

echo "Changing directory to build folder..."
cd %binDir%
