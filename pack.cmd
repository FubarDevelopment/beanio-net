@echo off
if exist "package-output" del "package-output" /f /q
for /d %%D in (src\*) do (
	dotnet pack %%D -c Release -o "package-output" --no-build --version-suffix "beta1-%1"
	if %errorlevel% neq  0 exit /b %errorlevel%
)
