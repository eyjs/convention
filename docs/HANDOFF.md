# 개발자 인수인계 문서

Convention Management System 개발/운영 인수인계를 위한 문서입니다.

## 1. 서버 환경

| 항목 | 값 |
|------|-----|
| 도메인 | https://event.ifa.co.kr |
| SSL | 설정 완료 |
| 파일 업로드 경로 | `D:\WebServer\event.ifa.co.kr\uploads` |
| DB 서버 | SQL Server (172.27.0.8) |
| DB명 | StarTour |
| 로그 | `logs/log.txt` (일별 롤링) |

## 2. 빌드 & 배포

### 백엔드 빌드

```bash
# 개발 빌드 (검증용)
dotnet build --no-restore

# 프로덕션 배포 빌드 (프론트엔드 자동 포함)
dotnet publish -c Release -o ./publish
```

`dotnet publish` 실행 시 `LocalRAG.csproj`의 `PublishVueApp` 타겟이:
1. `ClientApp/` 에서 `npm install --legacy-peer-deps` 실행
2. `npm run build` 실행
3. `ClientApp/dist/**` → `wwwroot/` 로 복사

### 프론트엔드 빌드 (단독)

```bash
cd ClientApp
npm install
npm run build       # dist/ 출력
npm run lint        # ESLint 검사
npm run format      # Prettier 포맷팅
```

- 빌드 설정: `vite.config.js`
- 환경변수: `.env.production` (VITE_API_URL, VITE_SIGNALR_HUB_URL)
- 빌드 출력: `ClientApp/dist/` → `dotnet publish` 시 자동으로 `wwwroot/`에 배치

### 주의사항

- `dotnet run` 실행 중에 `dotnet build` 하면 MSB3027 파일 잠김 에러 발생 — CS 에러 아님, 무시 가능
- 실제 CS 컴파일 에러 확인: `dotnet build 2>&1 | grep "error CS"`
- 기존 경고 46개 (CS8618 nullable, CS0618 obsolete) — 기존 것이므로 무시

## 3. DB 마이그레이션

### 마이그레이션 생성 & 적용

```bash
# 1. dotnet run이 실행 중이면 먼저 종료
# 2. 마이그레이션 생성
dotnet ef migrations add <MigrationName>

# 3. DB에 적용
dotnet ef database update

# 4. 마이그레이션 롤백 (직전 상태로)
dotnet ef database update <PreviousMigrationName>

# 5. 마이그레이션 제거 (아직 적용 안 된 경우)
dotnet ef migrations remove
```

### Entity 수정 시 영향 범위

Entity 클래스를 수정하면 다음이 영향을 받습니다:

```
Entity 수정 (Entities/*.cs)
  │
  ├── Data/Configurations/*Configuration.cs  ← FK/인덱스 설정 변경 필요할 수 있음
  ├── Data/ConventionDbContext.cs             ← 새 엔티티면 DbSet 추가 필요
  ├── Migrations/                            ← dotnet ef migrations add 필요
  │
  ├── Repositories/IUnitOfWork.cs            ← 새 엔티티면 IRepository<T> 프로퍼티 추가
  ├── Repositories/UnitOfWork.cs             ← 위에 대응하는 구현 추가
  │
  ├── DTOs/*Models/*.cs                      ← DTO에 새 필드 반영
  ├── Services/**/*Service.cs                ← 비즈니스 로직에서 새 필드 매핑
  ├── Interfaces/I*Service.cs                ← 서비스 시그니처 변경 시
  ├── Controllers/**/*Controller.cs          ← 엔드포인트 변경 시
  │
  └── ClientApp/src/                         ← 프론트엔드 API 호출부/UI 수정
```

### DB 수동 업데이트 (마이그레이션 없이 직접 ALTER)

수동으로 DB 스키마를 변경한 경우:
1. Entity 클래스를 DB와 동일하게 수정
2. `dotnet ef migrations add <Name>` → 빈 마이그레이션이 생성되어야 정상 (스냅샷 동기화)
3. 빈 마이그레이션이 아니면 Entity와 DB가 불일치 — 확인 필요

## 4. 인증 체계

### 토큰 설정

| 항목 | 개발 | 프로덕션 |
|------|------|---------|
| AccessToken 만료 | 7일 (10080분) | 7일 (10080분) |
| RefreshToken 만료 | 7일 | 14일 |
| JWT SecretKey | appsettings.json 기본값 | appsettings.Production.json (별도) |

### 토큰 갱신 흐름

```
1. 클라이언트 API 요청 → 401 Unauthorized
2. Axios 인터셉터 감지 → POST /api/auth/refresh (RefreshToken 전송)
3. 서버: RefreshToken DB 조회 → 유효하면 새 AccessToken + 새 RefreshToken 발급
4. 새 토큰 localStorage 저장 → 원래 요청 재시도
5. RefreshToken도 만료됐으면 → localStorage 클리어 → /login 리다이렉트
```

- RefreshToken은 **회전(rotation)** 방식: 갱신할 때마다 새 토큰 발급, 기존 토큰 무효화
- 동시 요청 race condition 방지: `failedQueue` 메커니즘 (api.js)
- Guest 로그인은 RefreshToken 미발급 → AccessToken 만료 시 재로그인 필요

### 역할 (Roles)

| 역할 | 설명 | 접근 범위 |
|------|------|----------|
| Admin | 관리자 | 전체 (`/api/admin/*` 포함) |
| User | 등록 사용자 (LoginId 있음) | 일반 API |
| Guest | 비회원 (LoginId 빈 문자열) | 일반 API (RefreshToken 없음) |

## 5. DI 등록 구조

새 서비스 추가 시 `Extensions/ServiceCollectionExtensions.cs`의 도메인별 메서드에 등록:

```csharp
// AddConventionServices() — 행사 관련 서비스
// AddAuthServices()       — 인증 관련 서비스
// AddAdminServices()      — 관리자 서비스
// AddChatServices()       — 채팅 서비스
// AddAiServices()         — AI/RAG 서비스
// AddUserServices()       — 사용자 서비스
// AddSmsServices()        — SMS 서비스
// AddUploadServices()     — 업로드 서비스
```

`Program.cs`에 직접 등록하지 않음.

## 6. 새 기능 추가 체크리스트

```
[ ] 1. Entity 생성 (Entities/)
[ ] 2. EntityTypeConfiguration 추가 (Data/Configurations/)
[ ] 3. ConventionDbContext에 DbSet 추가
[ ] 4. dotnet ef migrations add → dotnet ef database update
[ ] 5. IUnitOfWork + UnitOfWork에 IRepository<T> 등록
[ ] 6. DTO 작성 (DTOs/)
[ ] 7. Interface 정의 (Interfaces/)
[ ] 8. Service 구현 (Services/)
[ ] 9. ServiceCollectionExtensions에 DI 등록
[ ] 10. Controller 작성 (Controllers/)
[ ] 11. dotnet build --no-restore 검증
[ ] 12. 프론트엔드 API 연동 + Vue 컴포넌트
[ ] 13. cd ClientApp && npm run build && npm run lint 검증
```

## 7. 프론트엔드 핵심 구조

### 상태 관리 (Pinia)

| 스토어 | 역할 |
|--------|------|
| auth | JWT 토큰, 사용자 정보, 로그인/로그아웃 |
| convention | 행사 선택, 일정/공지/사진 로드 |
| feature | 행사별 기능 플래그 |
| theme | 7개 프리셋 테마 + 커스텀 색상 |
| popup | 전역 팝업 (컴포넌트 동적 렌더링) |
| ui | 모달 스택 관리 |

### 라우터 가드

- `requiresAuth` — JWT 인증 필수
- `requiresAdmin` — Admin 역할 필수
- `requiresConvention` — 행사 선택 필수

### 주요 Composable

| 파일 | 용도 |
|------|------|
| useAction.js | Dynamic Action behaviorType별 라우팅 |
| useAdminNav.js | 관리자 사이드바 네비게이션 |
| useSurveyManagement.js | 설문 관리 공통 로직 (일반/옵션투어) |
| useToast.js | 토스트 알림 |
| useDevice.js | 모바일/데스크탑 감지 |

## 8. 파일 업로드 구조

- 업로드 경로: `StorageSettings:FileUploadPath` (appsettings.json)
- 프로덕션: `D:\WebServer\event.ifa.co.kr\uploads`
- 파일 접근: `/api/file/viewer/{year}/{dayOfYear}/{fileName}` (Controller가 직접 서빙)
- wwwroot/uploads: FormBuilder 전용 (별도 경로)

## 9. 알려진 이슈 / 기술 부채

| 항목 | 상태 | 설명 |
|------|------|------|
| DeleteYn 패턴 | 미완료 | "N"/"Y" 또는 "1"/"2" 혼재 — DB 마이그레이션 필요 |
| PersonalTripService | 미완료 | 893줄, 서브 서비스 분리 필요 |
| localStorage 토큰 | 개선 가능 | XSS 취약 — HttpOnly 쿠키 전환 권장 |
| 멀티탭 로그아웃 | 미구현 | 다른 탭에서 로그아웃 시 동기화 안 됨 |

## 10. 운영 모니터링

- 헬스체크: `GET /health` (DB 상태 확인)
- 로그: `logs/log.txt` (Serilog, 일별 롤링)
- Swagger: 개발 환경에서만 활성화 (`/swagger`)
