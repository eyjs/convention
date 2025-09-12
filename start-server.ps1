# 스타투어 AI 챗봇 - 마스터 실행 스크립트

param(
    [switch]$SkipOllama,
    [switch]$SkipDatabase,
    [string]$Port = "7070"
)

Write-Host "🌟 스타투어 AI 챗봇 시작 🌟" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

$projectPath = $PSScriptRoot
Set-Location $projectPath

# 1. Ollama 설정 (건너뛰기 옵션)
if (-not $SkipOllama) {
    Write-Host "`n📦 Ollama 설정 시작..." -ForegroundColor Cyan
    & ".\setup-ollama.ps1"
} else {
    Write-Host "`n📦 Ollama 설정 건너뛰기" -ForegroundColor Yellow
}

# 2. 데이터베이스 설정 (건너뛰기 옵션)
if (-not $SkipDatabase) {
    Write-Host "`n🗄️ 데이터베이스 설정 시작..." -ForegroundColor Cyan
    & ".\setup-database.ps1"
} else {
    Write-Host "`n🗄️ 데이터베이스 설정 건너뛰기" -ForegroundColor Yellow
}

# 3. 프로젝트 최종 빌드
Write-Host "`n🔨 프로젝트 최종 빌드..." -ForegroundColor Cyan
dotnet build --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ 빌드 실패!" -ForegroundColor Red
    Read-Host "Enter를 눌러 종료..."
    exit 1
}

Write-Host "✅ 빌드 성공!" -ForegroundColor Green

# 4. 서버 실행
Write-Host "`n🚀 서버 실행 중..." -ForegroundColor Cyan
Write-Host "포트: $Port" -ForegroundColor Yellow
Write-Host "중지하려면 Ctrl+C를 누르세요" -ForegroundColor Yellow

Write-Host "`n📋 접속 URL:" -ForegroundColor Cyan
Write-Host "  • 테스트 페이지: https://localhost:$Port" -ForegroundColor White
Write-Host "  • API 문서: https://localhost:$Port/swagger" -ForegroundColor White
Write-Host "  • Health Check: https://localhost:$Port/api/llm/providers" -ForegroundColor White

Write-Host "`n🔧 개발 팁:" -ForegroundColor Cyan
Write-Host "  • 코드 변경시 자동 재시작: dotnet watch run" -ForegroundColor White
Write-Host "  • 디버그 모드: F5 (Visual Studio)" -ForegroundColor White
Write-Host "  • 로그 확인: 콘솔 출력 모니터링" -ForegroundColor White

# 환경 변수 설정
$env:ASPNETCORE_URLS = "https://localhost:$Port;http://localhost:$([int]$Port + 1)"

# 서버 시작
try {
    dotnet run --configuration Release
} catch {
    Write-Host "`n❌ 서버 실행 중 오류 발생!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "`n👋 서버가 종료되었습니다." -ForegroundColor Yellow