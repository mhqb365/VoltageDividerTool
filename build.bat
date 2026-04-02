@echo off
setlocal enabledelayedexpansion

echo [1/3] Cleaning...
if exist "dist" rd /s /q "dist"
mkdir "dist"

echo [2/3] Compiling...
dotnet build VoltageDividerTool.csproj -c Release --nologo -v q

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Build failed! Please check the code
    pause
    exit /b %ERRORLEVEL%
)

echo [3/3] Copying files...
copy /y "bin\Release\net48\VoltageDividerTool.exe" "dist\Phan mem chia ap.exe" > nul
copy /y "bin\Release\net48\VoltageDividerTool.exe" "dist\Voltage Divider Tool.exe" > nul

echo.
echo ======================================================
echo CONGRATULATIONS! Build completed
echo Files are located at: dist
echo ======================================================
echo.
pause
