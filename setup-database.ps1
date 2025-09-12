# 스타투어 AI 챗봇 - 데이터베이스 설정 스크립트

Write-Host "=== 스타투어 AI 챗봇 데이터베이스 설정 ===" -ForegroundColor Green

# 프로젝트 디렉토리로 이동
$projectPath = "C:\Users\USER\dev\chatbot\LocalRAG"
Set-Location $projectPath

Write-Host "현재 디렉토리: $(Get-Location)" -ForegroundColor Yellow

# EF Core Tools 확인 및 설치
Write-Host "`n1. EF Core Tools 확인 중..." -ForegroundColor Cyan
try {
    $efVersion = dotnet ef --version
    Write-Host "EF Core Tools 버전: $efVersion" -ForegroundColor Green
} catch {
    Write-Host "EF Core Tools 설치 중..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

# 프로젝트 복원
Write-Host "`n2. 프로젝트 의존성 복원 중..." -ForegroundColor Cyan
dotnet restore

# 마이그레이션 생성
Write-Host "`n3. 데이터베이스 마이그레이션 생성 중..." -ForegroundColor Cyan
try {
    dotnet ef migrations add InitialCreate
    Write-Host "마이그레이션 생성 완료!" -ForegroundColor Green
} catch {
    Write-Host "마이그레이션이 이미 존재하거나 오류가 발생했습니다." -ForegroundColor Yellow
}

# 데이터베이스 업데이트
Write-Host "`n4. 데이터베이스 업데이트 중..." -ForegroundColor Cyan
try {
    dotnet ef database update
    Write-Host "데이터베이스 업데이트 완료!" -ForegroundColor Green
} catch {
    Write-Host "데이터베이스 업데이트 중 오류가 발생했습니다." -ForegroundColor Red
    Write-Host "SQL Server LocalDB가 설치되어 있는지 확인하세요." -ForegroundColor Yellow
}

# 프로젝트 빌드
Write-Host "`n5. 프로젝트 빌드 중..." -ForegroundColor Cyan
dotnet build

Write-Host "`n=== 설정 완료 ===" -ForegroundColor Green
Write-Host "다음 명령으로 서버를 시작하세요:" -ForegroundColor Yellow
Write-Host "dotnet run" -ForegroundColor White
Write-Host "`n테스트 페이지: https://localhost:7070" -ForegroundColor Cyan
Write-Host "API 문서: https://localhost:7070/swagger" -ForegroundColor Cyan

Read-Host "`n계속하려면 Enter를 누르세요..."