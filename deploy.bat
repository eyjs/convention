@echo off
chcp 65001 >nul
title 스타투어 AI 챗봇 - Git 자동 배포
color 0A

echo ========================================
echo      🚀 스타투어 AI 챗봇 Git 배포 🚀
echo ========================================
echo.

REM cd /d "C:\Users\USER\dev\chatbot\LocalRAG"

echo 📂 현재 디렉토리: %CD%
echo.

REM Git 상태 확인
echo 🔍 Git 상태 확인 중...
git status
echo.

REM 원격 저장소에서 최신 변경사항 가져오기
echo 📥 원격 저장소에서 최신 변경사항 가져오는 중...
git fetch origin
if %ERRORLEVEL% neq 0 (
    echo ❌ Git fetch 실패!
    echo 원격 저장소 연결을 확인하세요.
    exit /b 1
)

echo ✅ Fetch 완료!
echo.

REM 로컬 변경사항이 있는지 확인
echo 🔍 로컬 변경사항 확인 중...
git diff --quiet
if %ERRORLEVEL% equ 0 (
    git diff --cached --quiet
    if %ERRORLEVEL% equ 0 (
        echo ℹ️  커밋할 변경사항이 없습니다.
        goto :pull_only
    )
)

echo 📝 변경사항이 발견되었습니다.
echo.

REM 변경된 파일 목록 표시
echo 📋 변경된 파일 목록:
git status --porcelain
echo.

REM 사용자에게 커밋 메시지 입력 받기
set "commit_msg=🔄 Auto deploy: Update project files"

echo.
echo 📦 변경사항을 스테이징하는 중...
git add .
if %ERRORLEVEL% neq 0 (
    echo ❌ Git add 실패!
    exit /b 1
)

echo ✅ 스테이징 완료!
echo.

echo 💾 커밋을 생성하는 중...
echo 메시지: "%commit_msg%"
git commit -m "%commit_msg%"
if %ERRORLEVEL% neq 0 (
    echo ❌ 커밋 생성 실패!
    exit /b 1
)

echo ✅ 커밋 생성 완료!
echo.

:pull_only
REM 원격 저장소에서 변경사항 pull
echo 📥 원격 저장소에서 변경사항을 가져오는 중...
git pull origin main --rebase
if %ERRORLEVEL% neq 0 (
    echo ⚠️  Pull 중 충돌이 발생했을 수 있습니다.
    echo 충돌을 해결한 후 다시 시도하세요.
    echo.
    echo 🔧 충돌 해결 명령어:
    echo   1. git status (충돌 파일 확인)
    echo   2. 충돌 파일 수동 편집
    echo   3. git add . (해결된 파일 스테이징)
    echo   4. git rebase --continue (리베이스 계속)
    echo.
    exit /b 1
)

echo ✅ Pull 완료!
echo.

REM 로컬에 새로운 커밋이 있는지 확인
git log origin/main..HEAD --oneline | find /c /v "" > nul
if %ERRORLEVEL% equ 0 (
    echo 📤 로컬 커밋을 원격 저장소에 푸시하는 중...
    git push origin main
    if %ERRORLEVEL% neq 0 (
        echo ❌ Push 실패!
        echo 인증 정보나 권한을 확인하세요.
        exit /b 1
    )
    echo ✅ Push 완료!
    echo.
) else (
    echo ℹ️  푸시할 새로운 커밋이 없습니다.
    echo.
)

REM 최종 상태 표시
echo 📊 최종 Git 상태:
git log --oneline -n 5
echo.

echo 🎉 배포 완료!
echo.
echo 🔗 유용한 링크:
echo   • GitHub 저장소: https://github.com/[USERNAME]/startour-ai-chatbot
echo   • Actions (CI/CD): https://github.com/[USERNAME]/startour-ai-chatbot/actions
echo   • Issues: https://github.com/[USERNAME]/startour-ai-chatbot/issues
echo.

echo 👋 배포가 완료되었습니다!
echo 아무 키나 누르면 창이 닫힙니다...
