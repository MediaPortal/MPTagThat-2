REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release 

echo off


set TARGETPATH="%1%..\Bin"

REM Copy All new files from base
xcopy %1\MPTagThat.Base\*.* %TARGETPATH% /E /R /Y /D


REM Copy Main Program
xcopy /y %1\MPTagThat\bin\%2\MPTagThat.exe %TARGETPATH%
xcopy /y %1\MPTagThat\bin\%2\MPTagThat.exe.* %TARGETPATH%
xcopy /y %1\MPTagThat\bin\%2\Prism.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat\bin\%2\Syncfusion.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat\bin\%2\System.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat\bin\%2\Unity.* %TARGETPATH%\Bin\
if "%2" == "Debug" (
  xcopy /y %1MPTagThat\bin\%2\MPTagThat.pdb %TARGETPATH%
)

REM Copy Core 
xcopy /y %1\MPTagThat.Core\bin\%2\MPTagThat.Core.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\NLog.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\taglib-sharp.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\NewtonSoft.Json.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\FreeImageNET.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\x64\FreeImage.dll %TARGETPATH%\Bin\x64\
xcopy /y %1\MPTagThat.Core\bin\%2\CommonServiceLocator.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\Hqub.MusicBrainz.API.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\Prism.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\Raven.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\Syncfusion.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\WPFLocalizeExtension.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\XAMLMarkupExtensions.* %TARGETPATH%\Bin\

REM Copy MicFiles Module
xcopy /y %1\UI\MPTagThat.MiscFiles\bin\%2\MPTagThat.MiscFiles.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.MiscFiles\bin\%2\Syncfusion.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.MiscFiles\bin\%2\System.Windows.* %TARGETPATH%\Bin\

REM Copy Ribbon Module
xcopy /y %1\UI\MPTagThat.Ribbon\bin\%2\MPTagThat.Ribbon.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.Ribbon\bin\%2\Syncfusion.* %TARGETPATH%\Bin\

REM Copy SongGrid Module
xcopy /y %1\UI\MPTagThat.SongGrid\bin\%2\MPTagThat.SongGrid.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.SongGrid\bin\%2\Syncfusion.* %TARGETPATH%\Bin\

REM Copy TagEdit Module
xcopy /y %1\UI\MPTagThat.TagEdit\bin\%2\MPTagThat.TagEdit.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.TagEdit\bin\%2\Syncfusion.* %TARGETPATH%\Bin\

REM Copy TreeView Module
xcopy /y %1\UI\MPTagThat.TreeView\bin\%2\MPTagThat.TreeView.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.TagEdit\bin\%2\Microsoft.Xaml.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.TreeView\bin\%2\Syncfusion.* %TARGETPATH%\Bin\

REM Copy Dialogs Module
xcopy /y %1\UI\MPTagThat.Dialogs\bin\%2\MPTagThat.Dialogs.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.Dialogs\bin\%2\Syncfusion.* %TARGETPATH%\Bin\