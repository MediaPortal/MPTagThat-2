@echo off
REM detect if PLATFORM should be x86 or x64
if not %1!==x86! goto X64
:x86
set PLATFORM="X86"
goto START
:X64
set PLATFORM="Any CPU"
goto START

:START
REM Select program path based on current machine environment
set progpath=%ProgramFiles%
if not "%ProgramFiles(x86)%".=="". set progpath=%ProgramFiles(x86)%

REM set other MP related paths
set GIT_ROOT=..\..
set DeployTool="%GIT_ROOT%\MPTagThat\DeployTool\DeployTool\bin\Debug\DeployTool.exe"
set MPTagThat=%GIT_ROOT%\MPTagThat\Source

REM Find the right version of msbuild on your system, by looking it up from "developer command prompt"
set MSBuild="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\msbuild.EXE"

REM set log file
set log=%project%.log


REM init log file, write dev env...
echo.
echo. > %log%
echo -= %project% =-
echo -= %project% =- >> %log%
echo -= build mode: %PLATFORM% =-
echo -= build mode: %PLATFORM% =- >> %log%
echo.
echo. >> %log%

REM Download NuGet packages
REM %MSBuild% RestorePackages.targets