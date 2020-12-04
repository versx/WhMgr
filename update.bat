@echo off

SET prjDir=%CD%\WhMgr
SET binDir=%prjDir%\bin\debug\netcoreapp2.1

:: Pull latest Git repository
echo "Pulling latest Git repository changes..."
git pull

:: Build WhMgr.dll
echo "Building WhMgr..."
dotnet build

:: Copy example config
::echo "Copying example files..."
::xcopy /s /e %prjDir%\examples\alerts\* %binDir%\alerts\
::xcopy /s /e %prjDir%\examples\filters\* %binDir%\filters\
::xcopy /s /e %prjDir%\examples\templates\* %binDir%\templates\

:: Copy locale translation files
echo "Copying locale translation files..."
xcopy /s /e %prjDir%\static\locale\* %binDir%\static\locale\

echo "Update Complete"