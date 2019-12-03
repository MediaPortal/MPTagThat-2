REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release 

echo off

REM Copy All new files from base
REM xcopy %1\MPTagThat.Base\*.* . /E /R /Y /D

set TARGETPATH="%1%..\Bin"

REM Copy MPTagThat Base Files
xcopy /y %1\MPTagThat\bin\%2\MPTagThat.exe %TARGETPATH%
xcopy /y %1\MPTagThat\bin\%2\MPTagThat.exe.* %TARGETPATH%
if "%2" == "Debug" (
  xcopy /y %1MPTagThat\bin\%2\MPTagThat.pdb %TARGETPATH%
)

REM Copy Core 
xcopy /y %1\MPTagThat.Core\bin\%2\MPTagThat.Core.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\NLog.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\taglib-sharp.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\NewtonSoft.Json.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\FreeImageNET.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\x64\FreeImage.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\CommonServiceLocator.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\Prism.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\Raven.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\Syncfusion.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\WPFLocalizeExtension.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\XAMLMarkupExtensions.* %TARGETPATH%\Bin\

REM Copy MicFiles Module
xcopy /y %1\UI\MPTagThat.MiscFiles\bin\%2\MPTagThat.MiscFiles.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.MiscFiles\bin\%2\Syncfusion.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.MiscFiles\bin\%2\System.Windows.* %TARGETPATH%\Bin\