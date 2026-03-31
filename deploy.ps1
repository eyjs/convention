# ============================================
# StarTour Convention 배포 스크립트
# 사용법: PowerShell에서 .\deploy.ps1
# ============================================

param(
    [string]$ServerPath = "\\172.25.0.21\D$\WebServer\event.ifa.co.kr",
    [switch]$IncludeDll,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$ClientApp = "$ProjectRoot\ClientApp"
$WwwrootSrc = "$ProjectRoot\wwwroot"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  StarTour Convention Deploy" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ---- 1. 프론트엔드 빌드 ----
if (-not $SkipBuild) {
    Write-Host "[1/4] 프론트엔드 빌드 중..." -ForegroundColor Yellow
    Push-Location $ClientApp
    npm run build
    if ($LASTEXITCODE -ne 0) { Pop-Location; throw "프론트엔드 빌드 실패" }
    Pop-Location

    Write-Host "[2/4] dist -> wwwroot 복사..." -ForegroundColor Yellow
    if (Test-Path $WwwrootSrc) { Remove-Item "$WwwrootSrc\*" -Recurse -Force }
    Copy-Item "$ClientApp\dist\*" $WwwrootSrc -Recurse -Force
    Write-Host "  완료" -ForegroundColor Green
} else {
    Write-Host "[1/4] 빌드 스킵 (-SkipBuild)" -ForegroundColor DarkGray
    Write-Host "[2/4] 빌드 스킵" -ForegroundColor DarkGray
}

# ---- 2. 서버 접근 확인 ----
Write-Host "[3/4] 서버 연결 확인..." -ForegroundColor Yellow
if (-not (Test-Path $ServerPath)) {
    Write-Host ""
    Write-Host "  서버 경로에 접근할 수 없습니다: $ServerPath" -ForegroundColor Red
    Write-Host "  VPN 연결 및 관리자 공유(D$) 접근 권한을 확인하세요." -ForegroundColor Red
    Write-Host ""
    Write-Host "  대안: RDP 접속 후 서버에서 직접 실행" -ForegroundColor Yellow
    Write-Host "  robocopy \\tsclient\C\Users\USER\dev\startour\convention\wwwroot D:\WebServer\event.ifa.co.kr\wwwroot /MIR /MT:8" -ForegroundColor White
    exit 1
}
Write-Host "  서버 연결 OK" -ForegroundColor Green

# ---- 3. 배포 ----
Write-Host "[4/4] 배포 중..." -ForegroundColor Yellow

# wwwroot (프론트엔드) — 항상 배포
Write-Host "  wwwroot 동기화..." -ForegroundColor Cyan
robocopy "$WwwrootSrc" "$ServerPath\wwwroot" /MIR /MT:8 /NJH /NJS /NDL /NP
Write-Host "  wwwroot 완료" -ForegroundColor Green

# Sample 폴더
Write-Host "  Sample 동기화..." -ForegroundColor Cyan
robocopy "$ProjectRoot\Sample" "$ServerPath\Sample" /MIR /MT:8 /NJH /NJS /NDL /NP
Write-Host "  Sample 완료" -ForegroundColor Green

# DLL (백엔드) — -IncludeDll 옵션일 때만
if ($IncludeDll) {
    Write-Host ""
    Write-Host "  [주의] DLL 배포 전 IIS 사이트를 중지하세요!" -ForegroundColor Red
    $confirm = Read-Host "  IIS 사이트 중지했습니까? (y/n)"
    if ($confirm -ne "y") {
        Write-Host "  DLL 배포를 건너뜁니다." -ForegroundColor Yellow
    } else {
        Write-Host "  DLL 동기화..." -ForegroundColor Cyan
        $PublishPath = "C:\Users\USER\dev\publish"

        # dotnet publish
        Push-Location $ProjectRoot
        dotnet publish -c Release -o $PublishPath --no-build
        Pop-Location

        robocopy "$PublishPath" "$ServerPath" *.dll *.exe *.json *.deps *.runtimeconfig /MT:8 /NJH /NJS /NDL /NP
        Write-Host "  DLL 완료" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  배포 완료!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

if (-not $IncludeDll) {
    Write-Host "  DLL 미포함 (프론트엔드만 배포됨)" -ForegroundColor DarkGray
    Write-Host "  백엔드 포함 배포: .\deploy.ps1 -IncludeDll" -ForegroundColor DarkGray
}
