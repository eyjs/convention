@echo off
echo ========================================
echo LocalDB Instance Information
echo ========================================
echo.

echo [1] All LocalDB Instances:
sqllocaldb info
echo.

echo [2] MSSQLLocalDB Instance Details:
sqllocaldb info mssqllocaldb
echo.

echo [3] Connection String for SSMS:
echo Server=(localdb)\mssqllocaldb;Integrated Security=true;
echo.

echo [4] Checking if startour database exists:
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT name FROM sys.databases WHERE name = 'startour'"
echo.

echo ========================================
echo To connect in SSMS, use:
echo Server Name: (localdb)\mssqllocaldb
echo Authentication: Windows Authentication
echo ========================================
pause
