@echo off
echo Compiling Sharua Task Manager...

set CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set OUTPUT=SharuaTaskManager.exe

REM Create output directory
if not exist "bin" mkdir "bin"

REM Compile the application
%CSC_PATH% /target:winexe /out:bin\%OUTPUT% /reference:System.Windows.Forms.dll,System.Drawing.dll /recurse:*.cs

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Compilation successful!
    echo Executable created: bin\%OUTPUT%
    echo.
    echo To run: bin\%OUTPUT%
    echo.
    echo Copying to root directory...
    copy "bin\%OUTPUT%" "%OUTPUT%"
    echo Done! You can now run %OUTPUT%
) else (
    echo.
    echo Compilation failed!
    echo Check the error messages above
)

pause