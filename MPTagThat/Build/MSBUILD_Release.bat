@echo OFF

Title Building MPTagThat (RELEASE)

rem build init
set project=MPTagThat
set version=%1
call BuildInit.bat %2

rem build
echo.
echo Writing GIT revision assemblies...
echo Writing GIT revision assemblies... >> %log%
%DeployTool% /git=%GIT_ROOT% /path=%MPTagThat% /UpdateVersion %version% >> %log%
if %ERRORLEVEL%==0 (
    echo %ERRORLEVEL%
	echo There are modifications in source folder. Aborting.
	pause
	exit 1
)

echo.
echo Building MPTagThat...
echo Building MPTagThat... >> %log%
%MSBuild% /target:Rebuild /property:Configuration=Release;Platform=%PLATFORM% "%MPTagThat%\MPTagThat.sln" >> %log%

echo.
echo Reverting assemblies...
echo Reverting assemblies... >> %log%
%DeployTool% /git="%GIT_ROOT%" /path="%MPTagThat%" /revert >> %log%

echo.
echo Reading the git revision...
echo Reading the git revision... >> %log%
SET /p build=<version.txt >> %log%
DEL version.txt >> %log%

echo.
echo Building Installer for Build %build% ...
echo Building Installer for Build %build% ... >> %log%
"%progpath%\NSIS\makensis.exe" "%GIT_ROOT%\MPTagThat\Setup\setup.nsi" >> %log%
ren "%GIT_ROOT%\MPTagThat\Setup\MPTagThat_setup.exe" "MPTagThat_%build%_setup.exe"