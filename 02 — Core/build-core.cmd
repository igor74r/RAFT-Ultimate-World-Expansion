[Reading 25 lines from start (total: 25 lines, 0 remaining)]

@echo off
setlocal
set "ROOT=%~dp0"
set "RAFT_DIR=%RAFT_DIR%"
if not defined RAFT_DIR set "RAFT_DIR=C:\Program Files (x86)\Steam\steamapps\common\Raft"
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set "OUT=%ROOT%artifacts\RaftUltimateWorldExpansion.Core.dll"

if not exist "%CSC%" (
  echo ERROR: .NET Framework C# compiler not found: "%CSC%"
  exit /b 2
)
if not exist "%RAFT_DIR%\Raft_Data\Managed\RaftModLoader_AssemblyLoader.dll" (
  echo ERROR: RML assembly not found under "%RAFT_DIR%"
  exit /b 3
)
if not exist "%RAFT_DIR%\Raft_Data\Managed\UnityEngine.CoreModule.dll" (
  echo ERROR: UnityEngine.CoreModule.dll not found under "%RAFT_DIR%"
  exit /b 4
)
if not exist "%ROOT%artifacts" mkdir "%ROOT%artifacts"

"%CSC%" /nologo /warnaserror /target:library /out:"%OUT%" /reference:"%RAFT_DIR%\Raft_Data\Managed\RaftModLoader_AssemblyLoader.dll" /reference:"%RAFT_DIR%\Raft_Data\Managed\UnityEngine.CoreModule.dll" "%ROOT%RaftUltimateWorldExpansion.cs"
if errorlevel 1 exit /b %ERRORLEVEL%
echo SUCCESS: "%OUT%"
exit /b 0

[executed on device: DESKTOP-3MKBODB (8088b63f-2a05-4f71-a7a4-ad740486ff11)]