#Requires -Version 5.1
<#
.SYNOPSIS
    전체 배포 (백엔드 + DB 마이그레이션 + 프론트엔드)

.DESCRIPTION
    1) dotnet publish
    2) DB 마이그레이션 (선택, -SkipMigration으로 건너뛰기)
    3) 백엔드 dll 복사 (앱풀 중지 상태에서 실행)
    4) 앱풀 시작 안내
    5) 프론트엔드(wwwroot) 복사 (무중단)
    6) 헬스체크

.EXAMPLE
    .\deploy.ps1                          # 전체 배포
    .\deploy.ps1 -SkipPublish             # publish 건너뛰기
    .\deploy.ps1 -SkipMigration           # DB 마이그레이션 건너뛰기
    .\deploy.ps1 -DryRun                  # 시뮬레이션

    # 개별 배포
    .\deploy-frontend.ps1                 # 프론트엔드만 (무중단)
    .\deploy-backend.ps1                  # 백엔드+DB만 (앱풀 중지 필요)
#>

param(
    [switch]$SkipPublish,
    [switch]$SkipMigration,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

# ============ 설정 ============
$ProjectRoot  = 'C:\Users\USER\dev\startour\convention'
$ClientApp    = Join-Path $ProjectRoot 'ClientApp'
$PublishDir   = 'C:\deploy\event'
$RemoteDrive  = 'W:'
$RemoteShare  = '\\172.25.0.21\webapp'
$RemoteUser   = '172.25.0.21\wnstn1342'
$RemotePass   = 'vmffpdl2@'
$HealthUrl    = 'https://event.ifa.co.kr/health'
$LogFile      = 'C:\deploy\deploy.log'
$HistoryDir   = 'C:\deploy\history'
$ProdConnStr  = 'Server=172.25.1.21;Database=STARTOUR;User Id=startour;Password=ifaelql!@#$;TrustServerCertificate=true;Encrypt=false;'

# 서버에서 절대 건드리면 안 되는 폴더 (CRITICAL)
$SafeDirs     = @('uploads', 'logs', 'App_Data')
$SafeFiles    = @('appsettings.Production.json', 'web.config')
# =============================

function Write-Step($msg) { Write-Host ""; Write-Host "▶ $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "  OK $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "  !! $msg" -ForegroundColor Yellow }
function Write-Err($msg)  { Write-Host "  XX $msg" -ForegroundColor Red }

$startTime = Get-Date
$timestamp = $startTime.ToString('yyyyMMdd_HHmmss')
if (-not (Test-Path (Split-Path $PublishDir))) { New-Item -ItemType Directory -Path (Split-Path $PublishDir) -Force | Out-Null }
if (-not (Test-Path $HistoryDir)) { New-Item -ItemType Directory -Path $HistoryDir -Force | Out-Null }

$script:reportLines = @()
function Add-Report($line) { $script:reportLines += $line }

Add-Report "============================================"
Add-Report " Full Deploy Report"
Add-Report "============================================"
Add-Report "Started    : $($startTime.ToString('yyyy-MM-dd HH:mm:ss'))"
Add-Report ""

# ─── 1. Publish ───────────────────────────────────────────
if (-not $SkipPublish) {
    Write-Step "1/7 dotnet publish"
    Push-Location $ProjectRoot
    try {
        $gitBranch = (git rev-parse --abbrev-ref HEAD 2>$null)
        $gitCommit = (git rev-parse --short HEAD 2>$null)
        $gitMsg    = (git log -1 --pretty=format:"%s" 2>$null)
        Add-Report "Git: $gitBranch @ $gitCommit ($gitMsg)"

        & dotnet publish -c Release -o $PublishDir
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed (exit $LASTEXITCODE)" }
        Write-Ok "publish 완료"

        # APK 복사 (있으면)
        $apkSource = Join-Path $ProjectRoot 'ClientApp\android\app\build\outputs\apk\debug\app-debug.apk'
        $apkDest = Join-Path $PublishDir 'wwwroot\downloads'
        if (Test-Path $apkSource) {
            if (-not (Test-Path $apkDest)) { New-Item -ItemType Directory -Path $apkDest -Force | Out-Null }
            Copy-Item $apkSource (Join-Path $apkDest 'StarTour.apk') -Force
            Write-Ok "APK 복사됨"
        }

        Add-Report "[1] Publish: OK"
    } finally { Pop-Location }
} else {
    Write-Step "1/7 publish 건너뜀"
    Add-Report "[1] Publish: SKIPPED"
}

# ─── 2. DB 마이그레이션 ───────────────────────────────────
if (-not $SkipMigration) {
    Write-Step "2/7 DB 마이그레이션 (운영 DB)"
    Write-Host "  대상: 172.25.1.21/STARTOUR" -ForegroundColor Gray

    if ($DryRun) {
        Write-Warn "DryRun — 마이그레이션 건너뜀"
        Add-Report "[2] Migration: DryRun SKIPPED"
    } else {
        Push-Location $ProjectRoot
        try {
            & dotnet ef database update --connection $ProdConnStr
            if ($LASTEXITCODE -ne 0) { throw "DB 마이그레이션 실패 (exit $LASTEXITCODE)" }
            Write-Ok "마이그레이션 완료"
            Add-Report "[2] Migration: OK"
        } finally { Pop-Location }
    }
} else {
    Write-Step "2/7 마이그레이션 건너뜀"
    Add-Report "[2] Migration: SKIPPED"
}

# ─── 3. 드라이브 매핑 ─────────────────────────────────────
Write-Step "3/7 원격 공유 확인"
$driveLetter = $RemoteDrive.TrimEnd(':')
if (-not (Get-PSDrive -Name $driveLetter -ErrorAction SilentlyContinue)) {
    & net use $RemoteDrive $RemoteShare /user:$RemoteUser $RemotePass | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "net use 실패" }
    Write-Ok "$RemoteDrive 매핑됨"
} else { Write-Ok "$RemoteDrive 이미 매핑됨" }
if (-not (Test-Path "$RemoteDrive\")) { throw "$RemoteDrive 접근 불가" }

# ─── 4. 백엔드 변경 감지 ──────────────────────────────────
Write-Step "4/7 백엔드 변경 감지"
$checkArgs = @(
    $PublishDir, "$RemoteDrive\",
    '/E', '/XO', '/L', '/NP', '/NFL', '/NDL', '/NJH',
    '/XD', ($SafeDirs + @('wwwroot')),
    '/XF', $SafeFiles
)
$null = & robocopy @checkArgs 2>&1
$backendChanged = $LASTEXITCODE -gt 0 -and $LASTEXITCODE -lt 8

if ($backendChanged) {
    Write-Warn "백엔드 변경 감지"
    Add-Report "[4] Backend: CHANGED"

    if (-not $DryRun) {
        Write-Host ""
        Write-Host "  백엔드 배포를 시작합니다." -ForegroundColor Yellow
        Write-Host "  앱풀 내렸나요? (Y/N): " -ForegroundColor Yellow -NoNewline
        $confirm = Read-Host
        if ($confirm -ne 'Y' -and $confirm -ne 'y') {
            Write-Err "앱풀을 먼저 중지하고 다시 실행하세요."
            throw "앱풀 미중지 — 배포 취소"
        }
        Write-Ok "앱풀 중지 확인됨"
    }

    # ─── 4a. 백엔드 복사 ──────────────────────────────
    Write-Step "4a/7 백엔드 dll 복사"
    $p1Start = Get-Date
    $p1Args = @(
        $PublishDir, "$RemoteDrive\",
        '/E', '/XO', '/R:1', '/W:1', '/NP',
        "/LOG:$LogFile", '/TEE',
        '/XD', ($SafeDirs + @('wwwroot')),
        '/XF', $SafeFiles
    )
    if ($DryRun) { $p1Args += '/L' }

    & robocopy @p1Args
    $p1Exit = $LASTEXITCODE
    $p1Elapsed = ((Get-Date) - $p1Start).TotalSeconds

    if ($p1Exit -ge 8) {
        Write-Err "백엔드 복사 실패 (exit $p1Exit)"
        Add-Report "[4a] Backend copy: FAILED"
        throw "배포 중단"
    }
    Write-Ok "백엔드 복사 완료 ($([math]::Round($p1Elapsed,1))초)"
    Add-Report "[4a] Backend copy: OK ($([math]::Round($p1Elapsed,1))s)"

    # ─── 5. 앱풀 시작 안내 ────────────────────────────
    Write-Step "5/7 앱풀 시작"
    Write-Host ""
    Write-Host "  ┌────────────────────────────────────┐" -ForegroundColor Yellow
    Write-Host "  │  백엔드 배포 완료!                  │" -ForegroundColor Yellow
    Write-Host "  │  서버 IIS에서 앱풀을 시작하세요.    │" -ForegroundColor Yellow
    Write-Host "  └────────────────────────────────────┘" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  프론트엔드 배포는 무중단으로 계속 진행됩니다..." -ForegroundColor Gray
} else {
    Write-Ok "백엔드 변경 없음 — 앱풀 중지 불필요"
    Add-Report "[4] Backend: NO CHANGES"
}

# ─── 6. 프론트엔드(wwwroot) 복사 — 무중단 ─────────────────
Write-Step "6/7 wwwroot 프론트엔드 복사 (무중단)"
$p2Start = Get-Date
$p2Args = @(
    "$PublishDir\wwwroot", "$RemoteDrive\wwwroot",
    '/E', '/XO', '/R:2', '/W:3', '/NP',
    "/LOG+:$LogFile", '/TEE',
    '/XD', $SafeDirs
)
if ($DryRun) { $p2Args += '/L' }

& robocopy @p2Args
$p2Exit = $LASTEXITCODE
$p2Elapsed = ((Get-Date) - $p2Start).TotalSeconds

if ($p2Exit -ge 8) {
    Write-Err "프론트엔드 복사 실패 (exit $p2Exit)"
    Add-Report "[6] Frontend: FAILED"
} else {
    Write-Ok "프론트엔드 복사 완료 ($([math]::Round($p2Elapsed,1))초)"
    Add-Report "[6] Frontend: OK ($([math]::Round($p2Elapsed,1))s)"
}

# ─── 7. 헬스체크 ──────────────────────────────────────────
Write-Step "7/7 헬스체크"
if ($DryRun) {
    Write-Warn "DryRun — 헬스체크 건너뜀"
} else {
    Write-Host "  대기..." -NoNewline
    $healthy = $false
    for ($i = 0; $i -lt 15; $i++) {
        try {
            $r = Invoke-WebRequest -Uri $HealthUrl -UseBasicParsing -TimeoutSec 3
            if ($r.StatusCode -eq 200) { $healthy = $true; break }
        } catch { }
        Start-Sleep -Seconds 2
        Write-Host "." -NoNewline
    }
    Write-Host ""
    if ($healthy) { Write-Ok "헬스체크 통과" }
    else { Write-Warn "헬스체크 실패 — 서버 로그 확인" }
}

$elapsed = (Get-Date) - $startTime
Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host " 전체 배포 완료 — $([int]$elapsed.TotalSeconds)초" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

Add-Report ""
Add-Report "Total: $([int]$elapsed.TotalSeconds)s — SUCCESS"
$reportFile = Join-Path $HistoryDir "deploy_$timestamp.txt"
$script:reportLines | Out-File -FilePath $reportFile -Encoding UTF8
Write-Host "리포트: $reportFile" -ForegroundColor Gray
