@echo off
echo ========================================
echo Database Setup and Migration
echo ========================================
echo.

echo [Step 1] Checking current connection...
echo Connection: Server=(localdb)\mssqllocaldb;Database=startour
echo.

echo [Step 2] Running EF Core Migration...
dotnet ef database update
echo.

IF %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ Migration completed successfully!
    echo.
    echo [Step 3] Verifying tables...
    sqlcmd -S "(localdb)\mssqllocaldb" -d startour -Q "SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"
    echo.
    
    echo [Step 4] Checking LlmSettings table...
    sqlcmd -S "(localdb)\mssqllocaldb" -d startour -Q "IF OBJECT_ID('LlmSettings', 'U') IS NOT NULL SELECT 'LlmSettings table EXISTS' as Status ELSE SELECT 'LlmSettings table NOT FOUND' as Status"
    echo.
    
    echo ========================================
    echo ✅ Setup Complete!
    echo ========================================
    echo.
    echo SSMS Connection:
    echo   Server: (localdb)\mssqllocaldb
    echo   Database: startour
) ELSE (
    echo.
    echo ❌ Migration failed! Check errors above.
)

echo.
pause
