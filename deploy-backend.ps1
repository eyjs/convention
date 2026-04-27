#Requires -Version 5.1
<#
.SYNOPSIS
    백엔드(dll + DB 마이그레이션) 배포 — 앱풀 중지 필요 (다운타임 발생)

.DESCRIPTION
    1) dotnet publish
    2) DB 마이그레이션 (운영 DB에 EF Core 적용)
    3) 앱풀 중지 확인 (Y/N)
    4) robocopy 백엔드 dll (wwwroot 제외)
    5) 앱풀 시작 안내
    6) 헬스체크

.EXAMPLE
    .\deploy-backend.ps1
    .\deploy-backend.ps1 -SkipPublish     # publish 건너뛰기
    .\deploy-backend.ps1 -SkipMigration   # DB 마이그레이션 건너뛰기
    .\deploy-backend.ps1 -DryRun          # 시뮬레이션
#>

param(
    [switch]$SkipPublish,
    [switch]$SkipMigration,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

# ============ 공통 설정 ============
$ProjectRoot  = 'C:\Users\USER\dev\startour\convention'
$PublishDir   = 'C:\deploy\event'
$RemoteDrive  = 'W:'
$RemoteShare  = '\\172.25.0.21\webapp'
$RemoteUser   = '172.25.0.21\wnstn1342'
$RemotePass   = 'vmffpdl2@'
$HealthUrl    = 'https://event.ifa.co.kr/health'
$LogFile      = 'C:\deploy\deploy.log'
$HistoryDir   = 'C:\deploy\history'
$ProdConnStr  = 'Server=172.25.1.21;Database=STARTOUR;User Id=startour;Password=ifaelql!@#$;TrustServerCertificate=true;Encrypt=false;'

# 서버에서 절대 건드리면 안 되는 폴더/파일 (CRITICAL)
$SafeDirs     = @('uploads', 'logs', 'App_Data', 'wwwroot')
$SafeFiles    = @('appsettings.Production.json', 'web.config')

function Write-Step($msg) { Write-Host ""; Write-Host "▶ $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "  OK $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "  !! $msg" -ForegroundColor Yellow }
function Write-Err($msg)  { Write-Host "  XX $msg" -ForegroundColor Red }

$startTime = Get-Date
$timestamp = $startTime.ToString('yyyyMMdd_HHmmss')
if (-not (Test-Path $HistoryDir)) { New-Item -ItemType Directory -Path $HistoryDir -Force | Out-Null }
$script:reportLines = @()
function Add-Report($line) { $script:reportLines += $line }

Add-Report "Backend Deploy Report — $($startTime.ToString('yyyy-MM-dd HH:mm:ss'))"
Add-Report ""

# ─── 1. Publish ───────────────────────────────────────────
if (-not $SkipPublish) {
    Write-Step "1/6 dotnet publish"
    Push-Location $ProjectRoot
    try {
        $gitBranch = (git rev-parse --abbrev-ref HEAD 2>$null)
        $gitCommit = (git rev-parse --short HEAD 2>$null)
        Add-Report "Git: $gitBranch @ $gitCommit"

        & dotnet publish -c Release -o $PublishDir
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed (exit $LASTEXITCODE)" }
        Write-Ok "publish 완료"
        Add-Report "[1] Publish: OK"
    } finally { Pop-Location }
} else {
    Write-Step "1/6 publish 건너뜀 (-SkipPublish)"
    Add-Report "[1] Publish: SKIPPED"
}

# ─── 2. DB 마이그레이션 ───────────────────────────────────
if (-not $SkipMigration) {
    Write-Step "2/6 DB 마이그레이션 (운영 DB)"
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
    Write-Step "2/6 마이그레이션 건너뜀 (-SkipMigration)"
    Add-Report "[2] Migration: SKIPPED"
}

# ─── 3. 드라이브 매핑 ─────────────────────────────────────
Write-Step "3/6 원격 공유 확인"
$driveLetter = $RemoteDrive.TrimEnd(':')
if (-not (Get-PSDrive -Name $driveLetter -ErrorAction SilentlyContinue)) {
    & net use $RemoteDrive $RemoteShare /user:$RemoteUser $RemotePass | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "net use 실패" }
    Write-Ok "$RemoteDrive 매핑됨"
} else { Write-Ok "$RemoteDrive 이미 매핑됨" }
if (-not (Test-Path "$RemoteDrive\")) { throw "$RemoteDrive 접근 불가" }

# ─── 4. 앱풀 확인 (Y/N) ──────────────────────────────────
Write-Step "4/6 앱풀 확인"
if (-not $DryRun) {
    Write-Host ""
    Write-Host "  백엔드 배포를 시작합니다." -ForegroundColor Yellow
    Write-Host "  앱풀 내렸나요? (Y/N): " -ForegroundColor Yellow -NoNewline
    $confirm = Read-Host
    if ($confirm -ne 'Y' -and $confirm -ne 'y') {
        Write-Err "앱풀을 먼저 중지하고 다시 실행하세요."
        Add-Report "[4] AppPool: NOT READY — 배포 취소"
        throw "앱풀 미중지 — 배포 취소"
    }
    Write-Ok "앱풀 중지 확인됨"
}

# ─── 5. 백엔드 복사 (wwwroot/uploads/logs 제외) ──────────
Write-Step "5/6 robocopy 백엔드 dll → 서버"
$copyStart = Get-Date
$copyArgs = @(
    $PublishDir, "$RemoteDrive\",
    '/E', '/XO', '/R:1', '/W:1', '/NP',
    "/LOG:$LogFile", '/TEE',
    '/XD', $SafeDirs,
    '/XF', $SafeFiles
)
if ($DryRun) { $copyArgs += '/L' }

& robocopy @copyArgs
$copyExit = $LASTEXITCODE
$copyElapsed = ((Get-Date) - $copyStart).TotalSeconds

if ($copyExit -ge 8) {
    Write-Err "robocopy 실패 (exit $copyExit)"
    Add-Report "[5] Robocopy: FAILED (exit=$copyExit)"
    throw "백엔드 배포 실패"
}
Write-Ok "백엔드 복사 완료 ($([math]::Round($copyElapsed,1))초)"
Add-Report "[5] Robocopy: OK (exit=$copyExit, $([math]::Round($copyElapsed,1))s)"

# ─── 5b. 앱풀 시작 안내 ──────────────────────────────────
Write-Step "5b/6 앱풀 시작"
Write-Host ""
Write-Host "  ┌────────────────────────────────────┐" -ForegroundColor Green
Write-Host "  │  백엔드 배포 완료!                  │" -ForegroundColor Green
Write-Host "  │  서버 IIS에서 앱풀을 시작하세요.    │" -ForegroundColor Green
Write-Host "  └────────────────────────────────────┘" -ForegroundColor Green
Write-Host ""

# ─── 6. 헬스체크 ──────────────────────────────────────────
Write-Step "6/6 헬스체크"
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
    else { Write-Warn "헬스체크 실패 — 앱풀 시작 후 다시 확인" }
}

$elapsed = (Get-Date) - $startTime
Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host " 백엔드 배포 완료 — $([int]$elapsed.TotalSeconds)초" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

Add-Report "Total: $([int]$elapsed.TotalSeconds)s — SUCCESS"
$reportFile = Join-Path $HistoryDir "deploy-backend_$timestamp.txt"
$script:reportLines | Out-File -FilePath $reportFile -Encoding UTF8
Write-Host "리포트: $reportFile" -ForegroundColor Gray
