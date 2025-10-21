@echo off
echo ========================================
echo EF Core 마이그레이션 적용 중...
echo ========================================

cd /d "D:\study\새 폴더\convention"

echo.
echo [1/2] 현재 마이그레이션 상태 확인 중...
dotnet ef migrations list

echo.
echo [2/2] 마이그레이션 적용 중...
dotnet ef database update

echo.
echo ========================================
echo 완료!
echo ========================================
pause
