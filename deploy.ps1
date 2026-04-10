#Requires -Version 5.1
<#
.SYNOPSIS
    event.ifa.co.kr 원클릭 배포 스크립트

.DESCRIPTION
    1) dotnet publish (로컬)
    2) 원격 IIS 앱풀 중지
    3) robocopy 전송
    4) 원격 IIS 앱풀 시작
    5) 헬스체크

.EXAMPLE
    .\deploy.ps1
    .\deploy.ps1 -SkipPublish   # publish 건너뛰기
    .\deploy.ps1 -DryRun        # robocopy /L 만 실행
#>

param(
    [switch]$SkipPublish,
    [switch]$DryRun,
    # 기본 true: 앱풀 원격 제어 건너뜀 (IIS에서 수동으로 내린 후 실행)
    [switch]$ManualAppPool = $true
)

$ErrorActionPreference = 'Stop'

# ============ 설정 ============
$ProjectRoot  = 'C:\Users\USER\dev\startour\convention'
$PublishDir   = 'C:\deploy\event'
$RemoteDrive  = 'W:'
$RemoteShare  = '\\172.25.0.21\webapp'
$RemoteUser   = '172.25.0.21\wnstn1342'
$RemotePass   = 'vmffpdl2@'
$ServerHost   = '172.25.0.21'
$AppPoolName  = 'event.ifa.co.kr'
$HealthUrl    = 'https://event.ifa.co.kr/health'
$LogFile      = 'C:\deploy\deploy.log'
$HistoryDir   = 'C:\deploy\history'

$ExcludeDirs  = @('logs', 'App_Data', 'wwwroot\uploads')
$ExcludeFiles = @('appsettings.Production.json', 'web.config')
# =============================

function Write-Step($msg) { Write-Host ""; Write-Host "▶ $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "  OK $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "  !! $msg" -ForegroundColor Yellow }
function Write-Err($msg)  { Write-Host "  XX $msg" -ForegroundColor Red }

$startTime = Get-Date
$timestamp = $startTime.ToString('yyyyMMdd_HHmmss')
$reportFile = Join-Path $HistoryDir "deploy_$timestamp.txt"
$script:reportLines = @()

function Add-Report($line) {
    $script:reportLines += $line
}

# deploy 폴더 생성
if (-not (Test-Path (Split-Path $PublishDir))) {
    New-Item -ItemType Directory -Path (Split-Path $PublishDir) -Force | Out-Null
}
if (-not (Test-Path $HistoryDir)) {
    New-Item -ItemType Directory -Path $HistoryDir -Force | Out-Null
}

Add-Report "============================================"
Add-Report " Deploy Report"
Add-Report "============================================"
Add-Report "Started    : $($startTime.ToString('yyyy-MM-dd HH:mm:ss'))"
Add-Report "Target     : $ServerHost ($AppPoolName)"
Add-Report "Source     : $ProjectRoot"
Add-Report "Publish    : $PublishDir"
Add-Report "Remote     : $RemoteShare"
Add-Report "DryRun     : $DryRun"
Add-Report "SkipPublish: $SkipPublish"
Add-Report ""

# ─── 1. Publish ───────────────────────────────────────────
if (-not $SkipPublish) {
    Write-Step "1/7 dotnet publish"
    Push-Location $ProjectRoot
    try {
        # git 정보 수집
        $gitBranch = (git rev-parse --abbrev-ref HEAD 2>$null)
        $gitCommit = (git rev-parse --short HEAD 2>$null)
        $gitMsg    = (git log -1 --pretty=format:"%s" 2>$null)
        Add-Report "[Git]"
        Add-Report "  branch : $gitBranch"
        Add-Report "  commit : $gitCommit"
        Add-Report "  message: $gitMsg"
        Add-Report ""

        dotnet publish -c Release -o $PublishDir
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed (exit $LASTEXITCODE)" }
        Write-Ok "publish 완료 -> $PublishDir"
        Add-Report "[1/5] Publish: SUCCESS"
    } finally {
        Pop-Location
    }
} else {
    Write-Step "1/7 publish 건너뜀 (-SkipPublish)"
    Add-Report "[1/5] Publish: SKIPPED"
}

# ─── 2. 드라이브 매핑 확인 ────────────────────────────────
Write-Step "2/7 원격 공유 매핑 확인"
$driveLetter = $RemoteDrive.TrimEnd(':')
$mapped = Get-PSDrive -Name $driveLetter -ErrorAction SilentlyContinue
if (-not $mapped) {
    Write-Warn "$RemoteDrive 미연결 — net use 실행"
    & net use $RemoteDrive $RemoteShare /user:$RemoteUser $RemotePass | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "net use 실패" }
    Write-Ok "$RemoteDrive -> $RemoteShare 매핑됨"
} else {
    Write-Ok "$RemoteDrive 이미 매핑됨"
}

if (-not (Test-Path "$RemoteDrive\")) { throw "$RemoteDrive 접근 불가" }

# 원격 세션 생성 — ManualAppPool 모드에서는 건너뜀
$session = $null
if (-not $DryRun -and -not $ManualAppPool) {
    try {
        $securePass = ConvertTo-SecureString $RemotePass -AsPlainText -Force
        $cred = New-Object System.Management.Automation.PSCredential($RemoteUser, $securePass)
        $session = New-PSSession -ComputerName $ServerHost -Credential $cred -ErrorAction Stop
    } catch {
        Write-Err "PSRemoting 연결 실패: $_"
        Write-Warn "수동 앱풀 제어로 전환합니다."
    }
}

# ─── 3. (앱풀 제어는 백엔드 변경 감지 후 필요 시에만 수행) ───

# ─── 4. 백엔드 변경 감지 + 스마트 배포 ─────────────────────
# 먼저 백엔드 dll이 변경되었는지 확인 (robocopy /L 드라이런)
Write-Step "4/7 백엔드 변경 감지"

$checkArgs = @(
    $PublishDir, "$RemoteDrive\",
    '/E', '/XO', '/L', '/NP', '/NFL', '/NDL', '/NJH',
    '/XD', 'wwwroot', 'logs', 'App_Data',
    '/XF', 'appsettings.Production.json', 'web.config'
)
$checkOutput = & robocopy @checkArgs 2>&1 | Out-String
$checkExit = $LASTEXITCODE

# exit 0 = 동일, 1+ = 변경 있음 (1=복사할 파일 있음, 2=추가 파일)
$backendChanged = $checkExit -gt 0 -and $checkExit -lt 8

if ($backendChanged) {
    Write-Warn "백엔드 파일 변경 감지 — 앱풀 중지 필요 (Phase 1)"
    Add-Report "[4/7] Backend: CHANGED (exit=$checkExit)"

    # ─── 4a. 앱풀 중지 ─────────────────────────────────
    if (-not $DryRun) {
        if ($ManualAppPool -or -not $session) {
            Write-Warn "서버 IIS에서 앱풀을 중지하세요."
            Write-Host "  앱풀 중지 후 Enter, 취소 Ctrl+C" -ForegroundColor Yellow
            Read-Host
        } else {
            Invoke-Command -Session $session -ScriptBlock {
                param($pool)
                Import-Module WebAdministration
                if ((Get-WebAppPoolState -Name $pool).Value -eq 'Started') {
                    Stop-WebAppPool -Name $pool
                    $t = 0
                    while ((Get-WebAppPoolState -Name $pool).Value -ne 'Stopped' -and $t -lt 15) {
                        Start-Sleep -Seconds 1; $t++
                    }
                }
            } -ArgumentList $AppPoolName
            Write-Ok "앱풀 중지됨"
            Start-Sleep -Seconds 2
        }
    }

    # ─── 4b. Phase 1: 백엔드 복사 ──────────────────────
    Write-Step "4b/7 Phase 1 — 백엔드 파일 복사"
    $phase1Start = Get-Date
    $phase1Args = @(
        $PublishDir, "$RemoteDrive\",
        '/E', '/XO', '/R:1', '/W:1', '/NP',
        "/LOG:$LogFile", '/TEE',
        '/XD', 'wwwroot', 'logs', 'App_Data',
        '/XF', 'appsettings.Production.json', 'web.config'
    )
    if ($DryRun) { $phase1Args += '/L' }

    & robocopy @phase1Args
    $phase1Exit = $LASTEXITCODE
    $phase1Elapsed = ((Get-Date) - $phase1Start).TotalSeconds

    Add-Report "[4b/7] Robocopy Phase 1 (backend): exit=$phase1Exit, elapsed=$([math]::Round($phase1Elapsed, 1))s"

    if ($phase1Exit -ge 8) {
        Write-Err "Phase 1 실패 (exit $phase1Exit)"
        Add-Report "[RESULT] FAILED at robocopy phase 1"
        $script:reportLines | Out-File -FilePath $reportFile -Encoding UTF8
        throw "배포 중단"
    }
    Write-Ok "Phase 1 완료 ($([math]::Round($phase1Elapsed, 1))초)"

    # ─── 4c. 앱풀 시작 ─────────────────────────────────
    Write-Step "5/7 앱풀 시작 (downtime 종료)"
    if (-not $DryRun) {
        if ($ManualAppPool -or -not $session) {
            Write-Warn "서버 IIS에서 앱풀을 **지금 바로** 시작하세요."
            Write-Host "  앱풀 시작 후 Enter" -ForegroundColor Yellow
            Read-Host
        } else {
            Invoke-Command -Session $session -ScriptBlock {
                param($pool)
                Import-Module WebAdministration
                Start-WebAppPool -Name $pool
            } -ArgumentList $AppPoolName
            Write-Ok "앱풀 시작됨"
            if ($session) { Remove-PSSession $session }
        }
    }
} else {
    Write-Ok "백엔드 변경 없음 — 앱풀 중지 불필요 (프론트만 배포)"
    Add-Report "[4/7] Backend: NO CHANGES — frontend-only deploy"
}

# ─── 6. Phase 2: 프론트엔드(wwwroot) 복사 — 사이트 가동 중 ──
Write-Step "6/7 robocopy Phase 2 — wwwroot 프론트엔드 (무중단)"
$phase2Start = Get-Date
$phase2Args = @(
    "$PublishDir\wwwroot", "$RemoteDrive\wwwroot",
    '/E', '/XO', '/R:2', '/W:3', '/NP',
    "/LOG+:$LogFile", '/TEE',
    '/XD', 'uploads'
)
if ($DryRun) { $phase2Args += '/L' }

& robocopy @phase2Args
$phase2Exit = $LASTEXITCODE
$phase2Elapsed = ((Get-Date) - $phase2Start).TotalSeconds

Add-Report "[6/7] Robocopy Phase 2 (wwwroot): exit=$phase2Exit, elapsed=$([math]::Round($phase2Elapsed, 1))s"

if ($phase2Exit -ge 8) {
    Write-Err "Phase 2 robocopy 실패 (exit $phase2Exit) — 백엔드는 이미 배포됨. 프론트 수동 확인 필요"
    Add-Report "[RESULT] PARTIAL — backend deployed, frontend failed"
} else {
    Write-Ok "Phase 2 완료 ($([math]::Round($phase2Elapsed, 1))초)"
}

# ─── 7. 헬스체크 ─────────────────────────────────────────
Write-Step "7/7 헬스체크"
if ($DryRun) {
    Write-Warn "DryRun — 헬스체크 건너뜀"
} else {
    Write-Host "  헬스체크 대기..." -NoNewline
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
    if ($healthy) {
        Write-Ok "헬스체크 통과 ($HealthUrl)"
    } else {
        Write-Warn "헬스체크 실패 — 서버 로그 확인 필요"
    }
}

$elapsed = (Get-Date) - $startTime
Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host " 배포 완료 — 소요시간: $([int]$elapsed.TotalSeconds)초" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

# 리포트 파일 저장
Add-Report "[RESULT] SUCCESS"
Add-Report ""
Add-Report "Finished   : $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))"
Add-Report "Elapsed    : $([int]$elapsed.TotalSeconds)s"
Add-Report "============================================"

$script:reportLines | Out-File -FilePath $reportFile -Encoding UTF8
Write-Host ""
Write-Host "리포트 저장: $reportFile" -ForegroundColor Gray
