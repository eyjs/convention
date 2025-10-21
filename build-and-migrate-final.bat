@echo off
chcp 65001 > nul
echo ========================================
echo 빌드 및 마이그레이션 실행
echo ========================================

cd /d "D:\study\새 폴더\convention"

echo.
echo [1/3] 프로젝트 정리 중...
dotnet clean

echo.
echo [2/3] 프로젝트 빌드 중...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ 빌드 실패! 오류를 확인하세요.
    echo.
    echo 주요 수정 사항:
    echo - ConventionDbContext.cs: 중복 DbSet 제거
    echo - ActionManagementController.cs: Convention.Name → Convention.Title
    echo - ActionTemplateController.cs: ?? 연산자 우선순위 수정
    pause
    exit /b 1
)

echo.
echo ✅ 빌드 성공!

echo.
echo [3/3] 마이그레이션 적용 중...
dotnet ef database update

if %ERRORLEVEL% EQ 0 (
    echo.
    echo ========================================
    echo ✅ 완료! 
    echo.
    echo 추가된 컬럼:
    echo - Guests.EnglishName (nvarchar(100))
    echo - Guests.PassportNumber (nvarchar(50))
    echo - Guests.PassportExpiryDate (date)
    echo - Guests.VisaDocumentAttachmentId (int)
    echo.
    echo 새로 생성된 테이블:
    echo - ConventionActions
    echo - GuestActionStatuses
    echo ========================================
) else (
    echo.
    echo ========================================
    echo ❌ 마이그레이션 실패! 
    echo 오류 메시지를 확인하세요.
    echo ========================================
)

echo.
echo 서버를 실행하려면 다음 명령어를 사용하세요:
echo dotnet run
echo.
pause
