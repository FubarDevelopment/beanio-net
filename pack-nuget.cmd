@echo off
if exist "package-output" del "package-output" /f /q
for %%f in (packaging\*.nuspec) do (
	packaging\nuget pack %%f -Properties "Configuration=Release;Id=%%~nf" -OutputDirectory "%~dp0package-output" -Version "4.0.0-beta1-%1"
	if %errorlevel% neq  0 exit /b %errorlevel%
)
