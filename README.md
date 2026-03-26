# Convention Management System

행사(컨벤션) 관리 시스템.

- **도메인**: https://event.ifa.co.kr
- **서버**: Windows Server 2core 4GB
- **백엔드**: .NET 8 / ASP.NET Core + EF Core 8 + SQL Server
- **프론트엔드**: Vue 3 (Composition API, JavaScript) + Vite + Tailwind CSS

## 주요 기능

| 기능 | 설명 |
|------|------|
| 행사 관리 | 컨벤션 생성/수정, 참석자(User) N:M 관리, 그룹 분류 |
| 일정 관리 | 템플릿 기반 일정, 이미지 갤러리, 옵션투어 |
| 설문조사 | 일반 설문 + 옵션투어 설문, 후속 질문, 통계/엑셀 내보내기 |
| Dynamic Action | DB 설정으로 프론트엔드 UI 동적 주입 (버튼/배너/팝업/카드) |
| 폼 빌더 | 커스텀 폼 생성/제출/관리 |
| SMS 발송 | 템플릿 기반 SMS 발송 |
| 개인 여행 | 항공편/숙소/일정/체크리스트 관리 |

## 빠른 시작

### 필수 환경

- .NET 8 SDK
- Node.js 18+
- SQL Server

### 개발 환경

```bash
# 백엔드
dotnet restore
dotnet run                    # http://localhost:5000

# 프론트엔드 (별도 터미널)
cd ClientApp
npm install
npm run dev                   # http://localhost:3000 → API 프록시 → :5000
```

### 프로덕션 빌드

```bash
# 전체 빌드 (프론트엔드 자동 포함)
dotnet publish -c Release -o ./publish

# 프론트엔드만 별도 빌드
cd ClientApp && npm run build    # dist/ → dotnet publish 시 wwwroot으로 복사
```

### DB 마이그레이션

```bash
# 주의: dotnet run 실행 중이면 파일 잠김 에러 발생 — 먼저 종료할 것
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## 프로젝트 구조

```
/
├── Controllers/              # API 컨트롤러
│   ├── Admin/                # 관리자 전용 (11개) — [Authorize(Roles = "Admin")]
│   ├── Auth/                 # 인증 (Login, Register, Guest, Refresh)
│   ├── Convention/           # 행사 관련 (Survey, Gallery 등)
│   ├── User/                 # 사용자 (Profile, Schedule)
│   └── File/                 # 파일 업로드/뷰어
├── Entities/                 # EF Core 엔티티 (43개)
├── DTOs/                     # 요청/응답 DTO
├── Services/                 # 비즈니스 로직 (48개)
├── Interfaces/               # 서비스 인터페이스 (34개)
├── Repositories/             # IUnitOfWork + IRepository<T> + 특화 Repository
├── Data/                     # ConventionDbContext + EntityTypeConfiguration (5개)
├── Migrations/               # EF Core 마이그레이션
├── ClientApp/src/            # Vue 3 프론트엔드
│   ├── services/             # Axios API 클라이언트
│   ├── components/           # 재사용 컴포넌트 (80+)
│   ├── views/                # 페이지 뷰 (43개)
│   ├── stores/               # Pinia 스토어 (7개: auth, convention, chat, feature, theme, popup, ui)
│   ├── composables/          # Vue Composable (10개)
│   ├── router/               # Vue Router (50+ 라우트)
│   └── dynamic-features/     # Dynamic Action 렌더러 + 레지스트리
└── docs/                     # 프로젝트 문서
```

## 핵심 아키텍처

### 도메인 모델 (N:M)

`User` ↔ `UserConvention` ↔ `Convention` 다대다 관계. `UserConvention`에 GroupName, AccessToken 등 행사별 맥락 저장.

- `User.LoginId` 빈 문자열 = Guest(비회원), 값 있음 = 등록 사용자
- `User.Role`: `"Admin"` / `"User"` / `"Guest"`

### Repository 패턴

- `IUnitOfWork` — 모든 Repository 단일 진입점
- `IRepository<T>` — GetByIdAsync, GetAsync, FindAsync, AddAsync, Update, Remove, Query(IQueryable)
- 트랜잭션: `ExecuteInTransactionAsync()` (SQL Server 재시도 전략 호환)
- 특화 Repository: IConventionRepository, IUserRepository 등 11개

### Dynamic Action System

관리자가 DB 설정만으로 프론트엔드 UI를 동적 주입:
1. `ConventionAction` → ActionCategory(BUTTON/BANNER/POPUP/CARD) + TargetLocation + ConfigJson
2. `DynamicActionRenderer.vue`가 componentMap으로 런타임 렌더링
3. `UserActionStatus`로 사용자별 완료/상태 추적

### 인증 체계

- JWT Bearer (HmacSha256)
- AccessToken 7일 + RefreshToken 14일 (Sliding: 갱신 시마다 14일 연장)
- Axios 401 인터셉터 → 자동 RefreshToken 갱신 (failedQueue로 race condition 방지)
- SignalR: query param `?access_token=` 방식

## 환경 설정

| 파일 | 용도 | Git |
|------|------|-----|
| `appsettings.json` | 기본 설정 (개발용 DB, JWT) | O |
| `appsettings.Production.json` | 프로덕션 오버라이드 (시크릿 키, DB) | X (.gitignore) |
| `ClientApp/.env.development` | 프론트 개발 환경변수 | O |
| `ClientApp/.env.production` | 프론트 프로덕션 환경변수 | O |

## API 구조

| 구분 | Route | 인증 |
|------|-------|------|
| 인증 | `/api/auth/*` | 불필요 |
| 행사 | `/api/conventions/*` | JWT |
| 관리자 | `/api/admin/*` | Admin |
| 설문 | `/api/surveys/*` | JWT |
| 파일 | `/api/files/*`, `/api/upload/*` | JWT |
| 개인여행 | `/api/personal-trips/*` | JWT |
| 헬스체크 | `/health` | 불필요 |
