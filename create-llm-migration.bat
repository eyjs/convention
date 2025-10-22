@echo off
echo Creating LlmSettings Migration...
dotnet ef migrations add AddLlmSettings

echo.
echo Applying Migration...
dotnet ef database update

echo.
echo Done!
pause
