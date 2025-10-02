# Git Commit & Push 스크립트
# Repository 패턴 구현 커밋

# 작업 디렉토리로 이동
Set-Location "C:\Users\USER\dev\convention"

Write-Host "=== Git 상태 확인 ===" -ForegroundColor Cyan
git status

Write-Host "`n=== 변경된 파일 추가 ===" -ForegroundColor Cyan
# Repositories 폴더의 모든 파일 추가
git add Repositories/

# Services 폴더의 ConventionService.cs 추가
git add Services/ConventionService.cs

# Controllers 폴더의 예시 컨트롤러 추가
git add Controllers/ConventionsExampleController.cs

Write-Host "`n=== 추가된 파일 확인 ===" -ForegroundColor Green
git status

Write-Host "`n=== 커밋 생성 ===" -ForegroundColor Cyan
$commitMessage = @"
feat: EF Core Repository 패턴 구현

- 11개 엔티티에 대한 Repository 구현 완료
- Unit of Work 패턴 적용
- 제네릭 Repository 구현 (기본 CRUD)
- 엔티티별 특화 Repository 구현
- DI 설정 및 확장 메서드 추가
- Service Layer 예시 (10가지 패턴)
- API Controller 예시
- 상세 문서 작성 (README, INSTALLATION, SUMMARY)

주요 기능:
- Convention, Guest, Schedule 등 11개 Repository
- 트랜잭션 관리 (BeginTransaction, Commit, Rollback)
- 페이징, 검색, Eager Loading 지원
- AsNoTracking 최적화
- Upsert, Bulk Insert 등 고급 패턴
"@

git commit -m $commitMessage

Write-Host "`n=== GitHub에 푸시 ===" -ForegroundColor Cyan
git push origin main

Write-Host "`n=== 완료! ===" -ForegroundColor Green
Write-Host "커밋이 성공적으로 푸시되었습니다!" -ForegroundColor Green
Write-Host "GitHub 저장소: https://github.com/eyjs/convention" -ForegroundColor Yellow

# 3초 대기 후 종료
Start-Sleep -Seconds 3
