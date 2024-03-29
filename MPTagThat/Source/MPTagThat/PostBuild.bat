REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release 

echo off


set TARGETPATH="%1%..\Bin"

REM Copy All new files from base
xcopy %1\MPTagThat.Base\bin\*.* %TARGETPATH%\Bin\ /E /R /D


REM Copy Main Program
xcopy /y %1\MPTagThat\bin\%2\MPTagThat.exe %TARGETPATH%
xcopy /y %1\MPTagThat\bin\%2\MPTagThat.exe.* %TARGETPATH%
xcopy /y %1\MPTagThat\bin\%2\Bass.Net.dll %TARGETPATH%\Bin\
xcopy /s /y %1\MPTagThat\bin\%2\x64\* %TARGETPATH%\Bin\x64\
xcopy /y %1\MPTagThat\bin\%2\MPTagThat.LicenseManager.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat\bin\%2\NewtonSoft.Json.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat\bin\%2\Prism.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat\bin\%2\Syncfusion.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat\bin\%2\System.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat\bin\%2\Unity.* %TARGETPATH%\Bin\
if "%2" == "Debug" (
  xcopy /y %1MPTagThat\bin\%2\MPTagThat.pdb %TARGETPATH%
)

REM Localization
xcopy /y %1\MPTagThat\bin\%2\de\* %TARGETPATH%\Localization\de\

REM Copy Core 
xcopy /y %1\MPTagThat.Core\bin\%2\MPTagThat.Core.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\NLog.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\taglibsharp.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\FreeImageNET.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\x64\FreeImage.dll %TARGETPATH%\Bin\x64\
xcopy /y %1\MPTagThat.Core\bin\%2\CommonServiceLocator.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\CSScriptLibrary.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\Hqub.MusicBrainz.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\DiscogsClient.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\IF.Lastfm.Core.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\LiteDB.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\Microsoft.* %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\Mono.CSharp.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\netstandard.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\RateLimiter.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\RestSharp.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\RestSharpHelper.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\System.Reactive.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\System.Data.SQLite.dll %TARGETPATH%\Bin\
xcopy /y %1\MPTagThat.Core\bin\%2\x64\SQLite.Interop.dll %TARGETPATH%\Bin\x64\
xcopy /y %1\MPTagThat.Core\bin\%2\Prism.* %TARGETPATH%\Bin\
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
xcopy /y %1\UI\MPTagThat.SongGrid\bin\%2\NReplayGain.dll %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.SongGrid\bin\%2\AcoustID.dll %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.SongGrid\bin\%2\x86\mp3val.exe %TARGETPATH%\Bin\

REM Copy TagEdit Module
xcopy /y %1\UI\MPTagThat.TagEdit\bin\%2\MPTagThat.TagEdit.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.TagEdit\bin\%2\Syncfusion.* %TARGETPATH%\Bin\

REM Copy TreeView Module
xcopy /y %1\UI\MPTagThat.TreeView\bin\%2\MPTagThat.TreeView.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.TagEdit\bin\%2\Microsoft.Xaml.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.TreeView\bin\%2\Syncfusion.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.TreeView\bin\%2\Interop.Shell32.dll %TARGETPATH%\Bin\

REM Copy Dialogs Module
xcopy /y %1\UI\MPTagThat.Dialogs\bin\%2\MPTagThat.Dialogs.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.Dialogs\bin\%2\Syncfusion.* %TARGETPATH%\Bin\

REM Copy Converter Module
xcopy /y %1\UI\MPTagThat.Converter\bin\%2\MPTagThat.Converter.* %TARGETPATH%\Bin\
xcopy /y %1\UI\MPTagThat.Converter\bin\%2\x64\*.* %TARGETPATH%\Bin\Encoder\

REM Copy Rip Module
xcopy /y %1\UI\MPTagThat.Rip\bin\%2\MPTagThat.Rip.* %TARGETPATH%\Bin\

REM Copy TagChecker Module
xcopy /y %1\UI\MPTagThat.TagChecker\bin\%2\MPTagThat.TagChecker.* %TARGETPATH%\Bin\
