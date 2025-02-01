@echo off
echo [INFO] Removing zone identifiers from files in directory %~dp0
%~dp0streams -d -s %~dp0
echo [INFO] Modifying Inkybot.exe to run as administrator by default %~dp0
reg add "HKCU\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers" /v "%~dp0Inkybot.exe" /t REG_SZ /d "~ RUNASADMIN" /f
echo [INFO] Installing dependency: Microsoft Visual C++ 2015-2019 Redistributable (x86)
"%~dp0VC_redist.x86.exe"
echo [INFO] Success
pause