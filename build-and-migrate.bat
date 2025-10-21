@echo off
echo ========================================
echo 빌드 및 마이그레이션 실행
echo ========================================

cd /d "D:\study\새 폴더\convention"

echo.
echo [1/3] 프로젝트 빌드 중...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ 빌드 실패! 오류를 확인하세요.
    pause
    exit /b 1
)

echo.
echo ✅ 빌드 성공!

echo.
echo [2/3] 현재 마이그레이션 상태 확인 중...
dotnet ef migrations list

echo.
echo [3/3] 마이그레이션 적용 중...
dotnet ef database update

if %ERRORLEVEL% EQ 0 (
    echo.
    echo ========================================
    echo ✅ 완료! EnglishName 컬럼이 추가되었습니다.
    echo ========================================
) else (
    echo.
    echo ========================================
    echo ❌ 마이그레이션 실패! 오류를 확인하세요.
    echo ========================================
)

echo.
pause
