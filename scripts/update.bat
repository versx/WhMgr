@echo off

SET prjDir=%CD%
SET binDir=%prjDir%\bin

:: Pull latest Git repository
echo "Pulling latest Git repository changes..."
git pull

:: Build WhMgr.dll
echo "Building WhMgr..."
dotnet build

:: Copy example config
::echo "Copying example files..."
::xcopy /s /e %prjDir%\examples\embeds\* %binDir%\embeds\
::xcopy /s /e %prjDir%\examples\filters\* %binDir%\filters\

echo "Copying latest master file..."
curl https://raw.githubusercontent.com/WatWowMap/Masterfile-Generator/master/master-latest.json > %binDir%\static\data\masterfile.json
xcopy /y /s /e /d %prjDir%\static\data\cpMultipliers.json %binDir%\static\data\cpMultipliers.json

echo "Update Complete"
