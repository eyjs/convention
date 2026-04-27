#Requires -Version 5.1
<#
.SYNOPSIS
    프론트엔드(wwwroot)만 배포 — 앱풀 중지 불필요 (무중단)

.EXAMPLE
    .\deploy-frontend.ps1
    .\deploy-frontend.ps1 -SkipBuild      # 빌드 건너뛰고 기존 publish로 복사만
    .\deploy-frontend.ps1 -DryRun         # 시뮬레이션
#>

param(
    [switch]$SkipBuild,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

# ============ 공통 설정 ============
$ProjectRoot  = 'C:\Users\USER\dev\startour\convention'
$ClientApp    = Join-Path $ProjectRoot 'ClientApp'
$PublishDir   = 'C:\deploy\event'
$RemoteDrive  = 'W:'
$RemoteShare  = '\\172.25.0.21\webapp'
$RemoteUser   = '172.25.0.21\wnstn1342'
$RemotePass   = 'vmffpdl2@'
$LogFile      = 'C:\deploy\deploy.log'
$HistoryDir   = 'C:\deploy\history'

function Write-Step($msg) { Write-Host ""; Write-Host "▶ $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "  OK $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "  !! $msg" -ForegroundColor Yellow }
function Write-Err($msg)  { Write-Host "  XX $msg" -ForegroundColor Red }

$startTime = Get-Date
$timestamp = $startTime.ToString('yyyyMMdd_HHmmss')
if (-not (Test-Path $HistoryDir)) { New-Item -ItemType Directory -Path $HistoryDir -Force | Out-Null }

# ─── 1. 빌드 ──────────────────────────────────────────────
if (-not $SkipBuild) {
    Write-Step "1/3 npm run build"
    Push-Location $ClientApp
    try {
        & npm run build
        if ($LASTEXITCODE -ne 0) { throw "npm run build failed (exit $LASTEXITCODE)" }
        Write-Ok "프론트엔드 빌드 완료"
    } finally { Pop-Location }

    Write-Step "1b/3 dotnet publish (wwwroot 포함)"
    Push-Location $ProjectRoot
    try {
        & dotnet publish -c Release -o $PublishDir
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed (exit $LASTEXITCODE)" }
        Write-Ok "publish 완료"
    } finally { Pop-Location }
} else {
    Write-Step "1/3 빌드 건너뜀 (-SkipBuild)"
    if (-not (Test-Path "$PublishDir\wwwroot")) {
        throw "publish 디렉토리에 wwwroot가 없습니다. -SkipBuild 없이 다시 실행하세요."
    }
}

# ─── 2. 드라이브 매핑 ─────────────────────────────────────
Write-Step "2/3 원격 공유 확인"
$driveLetter = $RemoteDrive.TrimEnd(':')
if (-not (Get-PSDrive -Name $driveLetter -ErrorAction SilentlyContinue)) {
    & net use $RemoteDrive $RemoteShare /user:$RemoteUser $RemotePass | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "net use 실패" }
    Write-Ok "$RemoteDrive 매핑됨"
} else { Write-Ok "$RemoteDrive 이미 매핑됨" }
if (-not (Test-Path "$RemoteDrive\")) { throw "$RemoteDrive 접근 불가" }

# ─── 3. wwwroot 복사 (무중단) ─────────────────────────────
Write-Step "3/3 robocopy wwwroot → 서버 (무중단)"
$copyStart = Get-Date
$copyArgs = @(
    "$PublishDir\wwwroot", "$RemoteDrive\wwwroot",
    '/E', '/XO', '/R:2', '/W:3', '/NP',
    "/LOG:$LogFile", '/TEE',
    '/XD', 'uploads'
)
if ($DryRun) { $copyArgs += '/L' }

& robocopy @copyArgs
$copyExit = $LASTEXITCODE
$copyElapsed = ((Get-Date) - $copyStart).TotalSeconds

if ($copyExit -ge 8) { throw "robocopy 실패 (exit $copyExit)" }

$elapsed = (Get-Date) - $startTime
Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host " 프론트엔드 배포 완료 — $([int]$elapsed.TotalSeconds)초 (무중단)" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

@(
    "Frontend Deploy — $($startTime.ToString('yyyy-MM-dd HH:mm:ss'))",
    "Robocopy: exit=$copyExit, $([math]::Round($copyElapsed,1))s",
    "Total: $([int]$elapsed.TotalSeconds)s — SUCCESS"
) | Out-File -FilePath (Join-Path $HistoryDir "deploy-frontend_$timestamp.txt") -Encoding UTF8
