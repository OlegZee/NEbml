@echo off

if exist "%VS120COMNTOOLS%vsvars32.bat" (
  @call "%VS120COMNTOOLS%vsvars32.bat"
  goto build
)

if exist "%VS110COMNTOOLS%vsvars32.bat" (
  @call "%VS110COMNTOOLS%vsvars32.bat"
  goto build
)

if exist "%VS100COMNTOOLS%vsvars32.bat" (
  @call "%VS100COMNTOOLS%vsvars32.bat"
  goto build
)

echo Requires VS2013 or VS2012 or VS2010 to be installed
goto exit

:build
msbuild nebml.sln /t:Rebuild /p:Configuration=Release

:exit