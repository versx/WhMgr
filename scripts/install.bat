@echo off

SET prjDir=%CD%\WhMgr
SET binDir=%prjDir%\bin

:: Download .NET 5.0 installer
echo "Downloading .NET 5.0 installer..."
powershell -Command "iwr -outf ~/Desktop/dotnet-install.ps1 https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.ps1"

:: Make installer executable

:: Install .NET 5.0 SDK
echo "Launching .NET installer..."
powershell -ExecutionPolicy RemoteSigned -File dotnet-install.ps1 -Version 5.0.404

:: Delete .NET 5.0 installer
echo "Deleting .NET installer..."
del dotnet-install.ps1

:: Clone repository
echo "Cloning repository..."
git clone https://github.com/versx/WhMgr -b v5-rewrite

:: Change directory into cloned repository
echo "Changing directory..."
cd %prjDir%

:: Build WhMgr.dll
echo "Building WhMgr..."
dotnet build

:: Copy example config
echo "Copying example files..."
xcopy /s /e %prjDir%\examples\discord_auth.json %binDir%\discord_auth.json
xcopy /s /e %prjDir%\examples\roles.example.json %binDir%\wwwroot\static\data\roles.json
xcopy /s /e %prjDir%\examples\configs\* %binDir%\configs\
xcopy /s /e %binDir%\configs\config.example.json %binDir%\configs\config.json
xcopy /s /e %prjDir%\examples\alarms\* %binDir%\alarms\
xcopy /s /e %prjDir%\examples\embeds\* %binDir%\embeds\
xcopy /s /e %prjDir%\examples\discords\* %binDir%\discords\
xcopy /s /e %prjDir%\examples\filters\* %binDir%\filters\
xcopy /s /e %prjDir%\examples\geofences\* %binDir%\geofences\
xcopy /s /e %prjDir%\static\* %binDir%\static\
curl https://raw.githubusercontent.com/WatWowMap/Masterfile-Generator/master/master-latest.json > %binDir%\static\data\masterfile.json

echo "Changing directory to build folder..."
cd %binDir%
