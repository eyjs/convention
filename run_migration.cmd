@echo off
chcp 65001 > nul
echo ============================================
echo  SeatingLayouts 마이그레이션 실행
echo ============================================
echo.
echo DB: 172.25.1.21 / STARTOUR
echo SQL: %~dp0seating_migration.sql
echo.
pause

sqlcmd -S 172.25.1.21 -U startour -P "ifaelql!@#$" -d STARTOUR -i "%~dp0seating_migration.sql"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [실패] 오류 코드: %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [성공] 마이그레이션 적용 완료. 검증 쿼리 실행...
echo.

sqlcmd -S 172.25.1.21 -U startour -P "ifaelql!@#$" -d STARTOUR -Q "SELECT COUNT(*) AS TableExists FROM sys.tables WHERE name='SeatingLayouts'; SELECT TOP 3 MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC;"

echo.
pause
