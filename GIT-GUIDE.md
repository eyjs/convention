# Git Commit & Push 가이드

## 🚀 빠른 실행

### **방법 1: PowerShell 스크립트 실행 (가장 쉬움)**

1. **PowerShell을 관리자 권한으로 실행**
   - Windows 검색 → "PowerShell" → 우클릭 → "관리자 권한으로 실행"

2. **스크립트 실행**
   ```powershell
   cd C:\Users\USER\dev\convention
   .\git-commit-push.ps1
   ```

   **만약 실행 정책 오류가 발생하면:**
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   .\git-commit-push.ps1
   ```

---

### **방법 2: Git Bash 사용**

1. **Git Bash 실행** (프로젝트 폴더에서 우클릭 → "Git Bash Here")

2. **다음 명령어 실행:**
   ```bash
   # 상태 확인
   git status
   
   # 파일 추가
   git add Repositories/
   git add Services/ConventionService.cs
   git add Controllers/ConventionsExampleController.cs
   
   # 커밋
   git commit -m "feat: EF Core Repository 패턴 구현

   - 11개 엔티티에 대한 Repository 구현 완료
   - Unit of Work 패턴 적용
   - 제네릭 Repository 구현 (기본 CRUD)
   - 엔티티별 특화 Repository 구현
   - DI 설정 및 확장 메서드 추가
   - Service Layer 예시 (10가지 패턴)
   - API Controller 예시
   - 상세 문서 작성 (README, INSTALLATION, SUMMARY)"
   
   # 푸시
   git push origin main
   ```

---

### **방법 3: VS Code에서 실행**

1. **VS Code 터미널 열기** (Ctrl + `)

2. **PowerShell 터미널에서 실행:**
   ```powershell
   cd C:\Users\USER\dev\convention
   .\git-commit-push.ps1
   ```

   또는 **Git 명령어 직접 입력:**
   ```powershell
   git add Repositories/ Services/ConventionService.cs Controllers/ConventionsExampleController.cs
   git commit -m "feat: EF Core Repository 패턴 구현"
   git push origin main
   ```

---

### **방법 4: Visual Studio에서 실행**

1. **팀 탐색기** 열기 (Ctrl + 0, Ctrl + M)
2. **변경 내용** 탭 선택
3. 추가할 파일 선택:
   - Repositories 폴더
   - Services/ConventionService.cs
   - Controllers/ConventionsExampleController.cs
4. **커밋 메시지 입력:**
   ```
   feat: EF Core Repository 패턴 구현
   ```
5. **커밋 후 푸시** 클릭

---

## 🔍 실행 전 확인사항

### 1. Git 상태 확인
```powershell
cd C:\Users\USER\dev\convention
git status
```

### 2. 추가된 파일 확인
다음 파일들이 추가되어야 합니다:
- ✅ Repositories/IRepository.cs
- ✅ Repositories/Repository.cs
- ✅ Repositories/IUnitOfWork.cs
- ✅ Repositories/UnitOfWork.cs
- ✅ Repositories/ConventionRepository.cs
- ✅ Repositories/GuestRepository.cs
- ✅ Repositories/ScheduleRepository.cs
- ✅ Repositories/GuestAttributeRepository.cs
- ✅ Repositories/SpecificRepositories.cs
- ✅ Repositories/RepositoryServiceExtensions.cs
- ✅ Repositories/README.md
- ✅ Repositories/INSTALLATION.md
- ✅ Repositories/SUMMARY.md
- ✅ Services/ConventionService.cs
- ✅ Controllers/ConventionsExampleController.cs

### 3. GitHub 인증 확인
- GitHub 계정에 로그인되어 있는지 확인
- SSH 키 또는 Personal Access Token 설정 확인

---

## ⚠️ 문제 해결

### 문제 1: "실행 정책 오류"
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### 문제 2: "인증 오류"
```powershell
# Personal Access Token 사용
git config --global credential.helper wincred

# 또는 SSH 키 사용
git remote set-url origin git@github.com:eyjs/convention.git
```

### 문제 3: "충돌 오류"
```powershell
# 최신 변경사항 먼저 가져오기
git pull origin main
# 충돌 해결 후
git add .
git commit -m "feat: EF Core Repository 패턴 구현"
git push origin main
```

### 문제 4: ".gitignore에 의해 무시됨"
```powershell
# 강제로 추가
git add -f Repositories/
```

---

## 📝 커밋 메시지 설명

```
feat: EF Core Repository 패턴 구현
```

- **feat**: 새로운 기능 추가
- **fix**: 버그 수정
- **docs**: 문서 수정
- **refactor**: 코드 리팩토링
- **test**: 테스트 코드

---

## ✅ 실행 후 확인

1. **GitHub에서 확인:**
   - https://github.com/eyjs/convention
   - Commits 탭에서 새 커밋 확인

2. **로컬에서 확인:**
   ```powershell
   git log --oneline -5
   ```

---

## 🎯 요약

**가장 쉬운 방법:**
1. PowerShell 관리자 권한 실행
2. `cd C:\Users\USER\dev\convention`
3. `.\git-commit-push.ps1`
4. 완료!

**문제 발생 시:**
- 실행 정책 오류 → `Set-ExecutionPolicy RemoteSigned`
- 인증 오류 → GitHub에 로그인 확인
- 충돌 오류 → `git pull` 먼저 실행
