@echo off
chcp 65001 > nul
echo ========================================
echo VectorDataEntry 마이그레이션 생성
echo ========================================

cd /d "D:\study\새 폴더\convention"

echo.
echo [1/2] 마이그레이션 생성 중...
dotnet ef migrations add AddSourceTypeAndTimestampsToVectorDataEntry

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ 마이그레이션 생성 실패!
    pause
    exit /b 1
)

echo.
echo ✅ 마이그레이션 생성 완료!

echo.
echo [2/2] 마이그레이션 적용 중...
dotnet ef database update

if %ERRORLEVEL% EQ 0 (
    echo.
    echo ========================================
    echo ✅ 완료! VectorDataEntry에 다음 필드가 추가되었습니다:
    echo - SourceType (nvarchar(50))
    echo - CreatedAt (datetime2)
    echo - UpdatedAt (datetime2)
    echo ========================================
) else (
    echo.
    echo ========================================
    echo ❌ 마이그레이션 적용 실패!
    echo ========================================
)

echo.
pause
