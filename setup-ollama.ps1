# 스타투어 AI 챗봇 - Ollama 설정 스크립트

Write-Host "=== 스타투어 AI 챗봇 Ollama 설정 ===" -ForegroundColor Green

# Ollama 설치 확인
Write-Host "`n1. Ollama 설치 확인 중..." -ForegroundColor Cyan
try {
    $ollamaVersion = ollama --version
    Write-Host "Ollama 버전: $ollamaVersion" -ForegroundColor Green
} catch {
    Write-Host "Ollama가 설치되지 않았습니다." -ForegroundColor Red
    Write-Host "다음 URL에서 Ollama를 다운로드하여 설치하세요:" -ForegroundColor Yellow
    Write-Host "https://ollama.ai/download" -ForegroundColor Cyan
    Read-Host "설치 완료 후 Enter를 누르세요..."
}

# Llama3 모델 확인 및 다운로드
Write-Host "`n2. Llama3 모델 확인 중..." -ForegroundColor Cyan
$models = ollama list 2>$null
if ($models -match "llama3") {
    Write-Host "Llama3 모델이 이미 설치되어 있습니다." -ForegroundColor Green
} else {
    Write-Host "Llama3 모델 다운로드 중... (시간이 오래 걸릴 수 있습니다)" -ForegroundColor Yellow
    ollama pull llama3
    Write-Host "Llama3 모델 다운로드 완료!" -ForegroundColor Green
}

# Ollama 서버 상태 확인
Write-Host "`n3. Ollama 서버 상태 확인 중..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:11434" -TimeoutSec 5 -ErrorAction Stop
    Write-Host "Ollama 서버가 실행 중입니다." -ForegroundColor Green
} catch {
    Write-Host "Ollama 서버가 실행되지 않았습니다." -ForegroundColor Yellow
    Write-Host "새 터미널에서 다음 명령을 실행하세요:" -ForegroundColor Cyan
    Write-Host "ollama serve" -ForegroundColor White
    
    $startServer = Read-Host "`n지금 Ollama 서버를 시작하시겠습니까? (y/n)"
    if ($startServer -eq "y" -or $startServer -eq "Y") {
        Write-Host "Ollama 서버를 백그라운드에서 시작합니다..." -ForegroundColor Yellow
        Start-Process PowerShell -ArgumentList "-Command", "ollama serve" -WindowStyle Minimized
        Start-Sleep -Seconds 3
        
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:11434" -TimeoutSec 10 -ErrorAction Stop
            Write-Host "Ollama 서버가 성공적으로 시작되었습니다!" -ForegroundColor Green
        } catch {
            Write-Host "Ollama 서버 시작에 실패했습니다. 수동으로 시작해주세요." -ForegroundColor Red
        }
    }
}

# 모델 테스트
Write-Host "`n4. Llama3 모델 테스트 중..." -ForegroundColor Cyan
try {
    Write-Host "간단한 테스트 질문을 보내는 중..." -ForegroundColor Yellow
    $testResult = ollama run llama3 "Hello, respond with just 'OK'" --verbose 2>$null
    if ($testResult -match "OK") {
        Write-Host "Llama3 모델이 정상적으로 작동합니다!" -ForegroundColor Green
    } else {
        Write-Host "모델 테스트 결과: $testResult" -ForegroundColor Yellow
    }
} catch {
    Write-Host "모델 테스트 중 오류가 발생했습니다." -ForegroundColor Yellow
    Write-Host "수동으로 테스트해보세요: ollama run llama3 'Hello'" -ForegroundColor Cyan
}

Write-Host "`n=== Ollama 설정 완료 ===" -ForegroundColor Green
Write-Host "Ollama 서버 URL: http://localhost:11434" -ForegroundColor Cyan
Write-Host "사용 가능한 모델: llama3" -ForegroundColor Cyan

# 설치된 모델 목록 표시
Write-Host "`n설치된 모델 목록:" -ForegroundColor Yellow
ollama list

Read-Host "`n계속하려면 Enter를 누르세요..."