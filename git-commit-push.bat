@echo off
chcp 65001 > nul
echo ========================================
echo    Git Commit ^& Push 스크립트
echo ========================================
echo.

cd /d "C:\Users\USER\dev\convention"

echo [1/5] Git 상태 확인 중...
git status
echo.

echo [2/5] 변경된 파일 추가 중...
git add Repositories/
git add Services/ConventionService.cs
git add Controllers/ConventionsExampleController.cs
echo ✓ 파일 추가 완료
echo.

echo [3/5] 추가된 파일 확인...
git status
echo.

echo [4/5] 커밋 생성 중...
git commit -m "feat: EF Core Repository 패턴 구현" -m "- 11개 엔티티에 대한 Repository 구현 완료" -m "- Unit of Work 패턴 적용" -m "- 제네릭 Repository 구현 (기본 CRUD)" -m "- 엔티티별 특화 Repository 구현" -m "- DI 설정 및 확장 메서드 추가" -m "- Service Layer 예시 (10가지 패턴)" -m "- API Controller 예시" -m "- 상세 문서 작성 (README, INSTALLATION, SUMMARY)"
echo ✓ 커밋 완료
echo.

echo [5/5] GitHub에 푸시 중...
git push origin main
echo.

if %errorlevel% equ 0 (
    echo ========================================
    echo    ✓ 성공적으로 푸시되었습니다!
    echo ========================================
    echo.
    echo GitHub 저장소: https://github.com/eyjs/convention
) else (
    echo ========================================
    echo    ✗ 오류가 발생했습니다
    echo ========================================
    echo.
    echo 위 오류 메시지를 확인하세요.
)

echo.
pause
