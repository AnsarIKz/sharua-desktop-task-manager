@echo off
echo Building Sharua Task Manager...

REM Clean previous builds
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"

REM Build the project
dotnet build -c Release

if %ERRORLEVEL% EQU 0 (
    echo Build successful!
    echo.
    echo To run the application:
    echo dotnet run
    echo.
    echo To create a standalone executable:
    echo dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
) else (
    echo Build failed!
    pause
)
