SET USRINPUT=
SET /p USRINPUT=Enter in the Alpha Number:

mkdir AVRProjectIDE_Alpha%USRINPUT%
mkdir AVRProjectIDE_Alpha%USRINPUT%\arduino
mkdir ArduinoFilesOnly
mkdir ArduinoFilesOnly\arduino
mkdir RequiredDLLsOnly

copy /Y /B AVRProjectIDEUpdater.exe AVRProjectIDE_Alpha%USRINPUT%\AVRProjectIDEUpdater.exe
copy /Y /B AVRProjectIDE.exe AVRProjectIDE_Alpha%USRINPUT%\AVRProjectIDE.exe
copy /Y /B AVRProjectIDE.exe AVRProjectIDE_LatestEXEOnly.exe
copy /Y /B AVRProjectIDE.exe AVRProjectIDE_Alpha%USRINPUT%.exe

copy /Y /B ScintillaNet.dll AVRProjectIDE_Alpha%USRINPUT%\ScintillaNet.dll
copy /Y /B SciLexer.dll AVRProjectIDE_Alpha%USRINPUT%\SciLexer.dll
copy /Y /B WeifenLuo.WinFormsUI.Docking.dll AVRProjectIDE_Alpha%USRINPUT%\WeifenLuo.WinFormsUI.Docking.dll

copy /Y /B ScintillaNet.dll RequiredDLLsOnly\ScintillaNet.dll
copy /Y /B SciLexer.dll RequiredDLLsOnly\SciLexer.dll
copy /Y /B WeifenLuo.WinFormsUI.Docking.dll RequiredDLLsOnly\WeifenLuo.WinFormsUI.Docking.dll

xcopy arduino\*.* AVRProjectIDE_Alpha%USRINPUT%\arduino\ /E /Y
xcopy arduino\*.* ArduinoFilesOnly\arduino\ /E /Y

pause