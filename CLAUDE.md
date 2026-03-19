# Convention Management System (LocalRAG)

행사(컨벤션) 관리 + RAG 기반 AI 챗봇 시스템. .NET 8 백엔드 + Vue 3 프론트엔드.

## 기술 스택

### 백엔드
- .NET 8 / ASP.NET Core (C#)
- Entity Framework Core 8 + SQL Server
- JWT 인증 (Bearer, HmacSha256)
- SignalR (실시간 채팅, `/chathub`)
- Serilog (파일 롤링 로깅)
- EPPlus (엑셀 처리)
- ONNX Runtime (임베딩)
- BCrypt (비밀번호 해싱)

### 프론트엔드 (`ClientApp/`)
- Vue 3 (Composition API, **JavaScript** — TypeScript 아님)
- Vite (빌드, 포트 3000)
- Pinia (상태 관리, 7개 스토어)
- Tailwind CSS
- Axios (HTTP, 자동 토큰 갱신)
- Vue Router (lazy loading, 50+ 라우트)
- Lucide (아이콘)
- Quill (리치 텍스트 에디터)
- ESLint v9 flat config (`eslint.config.js`)

## 실행 명령어

```bash
# 백엔드
dotnet restore
dotnet run                    # http://localhost:5000
dotnet build --no-restore     # 빌드 검증 (restore 이미 완료 시)

# 프론트엔드
cd ClientApp
npm install
npm run dev                   # http://localhost:3000 (API 프록시 -> :5000)
npm run build                 # ClientApp/dist 출력
npm run lint                  # ESLint 검사
npm run format                # Prettier 포맷팅

# DB 마이그레이션
dotnet ef migrations add <Name>
dotnet ef database update

# 프로덕션 빌드
dotnet publish                # 프론트엔드 자동 빌드 포함 (dist → wwwroot)
```

## 프로젝트 구조

```
/
├── Controllers/              # API 컨트롤러 (/api/*)
│   ├── Admin/                # 관리자 API (11개) — [Authorize(Roles = "Admin")]
│   │   ├── AdminUserController        # 참석자 CRUD, 사용자 관리, 기존사용자 링크
│   │   ├── AdminScheduleController    # 일정 템플릿 관리
│   │   ├── AdminNoticesController     # 공지사항 관리
│   │   ├── AdminSmsController         # SMS 템플릿/발송
│   │   ├── AdminStatsController       # 통계
│   │   ├── AdminAttributeController   # 속성 정의 관리
│   │   ├── ActionManagementController # Dynamic Action 관리
│   │   ├── ActionTemplateController   # 액션 템플릿
│   │   ├── ChatbotManagementController# LLM/인덱싱 관리
│   │   ├── DatabaseController         # DB 유틸리티
│   │   └── FormBuilderAdminController # 폼 빌더 관리
│   ├── Ai/                   # AI/RAG (ConventionChat, Indexing, Rag)
│   ├── Auth/                 # 인증 (Auth, AccountRecovery, Setup)
│   ├── Convention/           # 행사 관련 (12개: Conventions, Survey, Gallery 등)
│   ├── Flight/               # 항공편 조회
│   ├── User/                 # 사용자 (User, UserSchedule)
│   └── File/                 # 파일 업로드 (File, Upload)
├── Entities/                 # EF Core 엔티티 (43개)
│   ├── Action/               # ConventionAction, UserActionStatus, ActionTemplate
│   ├── Flight/               # IncheonFlightData
│   ├── FormBuilder/          # FormDefinition, FormField, FormSubmission
│   ├── PersonalTrip/         # PersonalTrip, Flight, Accommodation, Itinerary, Checklist
│   ├── Survey/               # Survey, SurveyQuestion, QuestionOption, Response
│   └── *.cs                  # User, Convention, UserConvention, Notice 등
├── DTOs/                     # 데이터 전송 객체
│   ├── AdminModels/          # UserDto, LinkUserDto, SmsDto 등
│   ├── ConventionModels/     # Convention CRUD 모델
│   ├── ScheduleModels/       # ScheduleTemplate, ScheduleItem (엔티티가 아닌 DTO)
│   └── ...
├── Services/                 # 비즈니스 로직 (48개)
│   ├── Admin/                # AdminUserService, AdminStatsService
│   ├── Ai/                   # IndexingService, LlmProviderManager, RagService
│   ├── Auth/                 # AuthService, VerificationService
│   ├── Chat/                 # ConventionChatService 외 7개
│   ├── Convention/           # ActionOrchestration, Schedule, Survey 등 9개
│   ├── Flight/               # FlightService
│   ├── FormBuilder/          # FormBuilderService
│   ├── PersonalTrip/         # PersonalTripService (893줄, 분리 필요)
│   ├── Shared/               # Checklist, SMS, FileUpload, UserContext 등 10개
│   ├── Upload/               # User/Schedule/Attribute/NameTag/OptionTour 업로드 6개
│   └── UserProfile/          # UserProfileService
├── Interfaces/               # 서비스 인터페이스 (34개, I*.cs)
├── Repositories/             # IUnitOfWork + IRepository<T> + 특화 Repository
├── Data/                     # ConventionDbContext (37 DbSets)
│   └── Configurations/       # IEntityTypeConfiguration 5개 (Action, Core, SMS 등)
├── Constants/                # Roles, DeleteStatus
├── Extensions/               # ClaimsPrincipalExtensions, ServiceCollectionExtensions
├── Middleware/                # GlobalException, RequestLogging
├── Providers/                # Llama3Provider, GeminiProvider
├── Hubs/                     # ChatHub (SignalR)
├── Storage/                  # MssqlVectorStore, InMemoryVectorStore
├── Configuration/            # JwtSettings, ConnectionStringProvider
├── Migrations/               # EF Core 마이그레이션
├── ClientApp/                # Vue 3 프론트엔드
│   └── src/
│       ├── services/         # API 클라이언트 (api.js + 10개 서비스)
│       ├── components/       # 재사용 컴포넌트 (80+개)
│       │   ├── admin/        # 관리자 (GuestManagement, BulkUpload 등)
│       │   │   ├── guest/    # GuestFormModal, GuestDetailModal, BulkOps, GroupOps
│       │   │   └── sms/      # SmsManagementModal
│       │   ├── chatbot/      # ChatWindow, ChatInput, ChatMessage 등
│       │   ├── common/       # BaseModal, SlideUpModal, DatePicker 등
│       │   ├── notice/       # NoticeFormModal, NoticeDetailModal
│       │   ├── personalTrip/ # FlightCard, AccommodationCard 등
│       │   └── trip/         # FlightSearchModal
│       ├── views/            # 페이지 뷰 (43개)
│       │   ├── admin/        # FormBuilder, NameTag, Survey 뷰
│       │   ├── trip/         # Trip CRUD, Itinerary, Expenses 등
│       │   ├── feature/      # Survey, GenericForm, DynamicForm
│       │   └── *.vue         # ConventionHome, Board, Login 등
│       ├── stores/           # Pinia 스토어 (7개)
│       │   ├── auth.js       # 인증, 토큰, 사용자
│       │   ├── convention.js # 행사 선택, 일정, 공지
│       │   ├── chat.js       # 채팅 메시지, 상태
│       │   ├── feature.js    # 기능 플래그
│       │   ├── theme.js      # 테마 (7개 프리셋 + 커스텀)
│       │   ├── popup.js      # 전역 팝업
│       │   └── ui.js         # 모달 스택, 채팅 토글
│       ├── composables/      # Vue Composable (8개: useAction, useDevice 등)
│       ├── router/           # 라우팅 (index.js, 50+ 라우트)
│       ├── dynamic-features/ # Dynamic Action 렌더러 + 레지스트리
│       │   ├── DynamicActionRenderer.vue  # 런타임 컴포넌트 렌더링
│       │   ├── registry.js               # 피처 자동 발견
│       │   └── common/                   # GenericButton, Banner, Card 등 6개
│       ├── layouts/          # DefaultLayout, FeatureLayout
│       ├── utils/            # date.js, fileUpload.js
│       └── schemas/          # actionCategories.js, targetLocations.js
└── wwwroot/                  # 정적 파일 (빌드 출력)
```

## 핵심 아키텍처

### 도메인 모델 (N:M)
- `User` ↔ `UserConvention` ↔ `Convention` (다대다 관계)
- `UserConvention`에 GroupName, AccessToken, LastChatReadTimestamp 저장
- `User.LoginId` 빈 문자열 = Guest(비회원), 값 있음 = 등록 사용자
- `User.Role`: "Admin" / "User" / "Guest" (`Constants/Roles.cs`)

### Repository 패턴 (IUnitOfWork)
- `IUnitOfWork` — 모든 Repository에 대한 단일 진입점
- 특화 Repository: `IConventionRepository`, `IUserRepository`, `IUserConventionRepository` 등 11개
- 제네릭 Repository: `IRepository<T>` — 35+ 엔티티
- 트랜잭션: `BeginTransactionAsync()` / `CommitTransactionAsync()` / `RollbackTransactionAsync()`
- `IRepository<T>` 메서드: `GetByIdAsync`, `GetAsync`, `FindAsync`, `AddAsync`, `Update`, `Remove`, `RemoveRange`, `Query`(IQueryable), `CountAsync`, `ExistsAsync`

### Dynamic Action System
- 관리자가 DB 설정만으로 프론트엔드 UI를 동적 주입
- `ConventionAction` → `ActionCategory`(BUTTON, BANNER, POPUP, CARD, CHECKLIST_CARD, MENU) + `TargetLocation` + `ConfigJson`
- `DynamicActionRenderer.vue`가 `componentMap`으로 런타임 동적 렌더링
- `UserActionStatus`로 사용자별 완료/상태 추적
- `useAction.js` composable: behaviorType별 라우팅 (FormBuilder, ModuleLink, Link 등)

### RAG AI 시스템
- `IEmbeddingService` → `OnnxEmbeddingService`(prod) / `LocalEmbeddingService`(dev)
- `MssqlVectorStore` — SQL Server 벡터 검색 (MAX_CANDIDATE_COUNT=500)
- `LlmProviderManager` — Llama3/Gemini 전환, `_cachedUpdatedAt` 캐시 무효화
- Chat Pipeline: `ConventionChatService` → `ChatIntentRouter` → `RagSearchService` → `LlmResponseService`

### DI 등록 구조 (`Extensions/ServiceCollectionExtensions.cs`)
- `AddDomainServices(config)` — 모든 서비스 일괄 등록
  - `AddAiServices()`, `AddChatServices()`, `AddAuthServices()`
  - `AddConventionServices()`, `AddUserServices()`, `AddAdminServices()`
  - `AddSmsServices()`, `AddUploadServices()`, `AddUserContextServices()`
- 새 서비스 추가 시 해당 도메인 메서드에 등록 (Program.cs 직접 수정 X)

### 미들웨어 파이프라인 순서
1. ForwardedHeaders → 2. HTTPS/HSTS (prod) → 3. GlobalExceptionMiddleware → 4. RequestLoggingMiddleware → 5. CORS → 6. StaticFiles → 7. Session → 8. Authentication → 9. Authorization → 10. HealthChecks(`/health`) → 11. SignalR(`/chathub`) → 12. Controllers

### 프론트엔드 상태 관리 (Pinia)
- `auth` — JWT 토큰, 사용자 정보, 로그인/로그아웃
- `convention` — 행사 선택, 일정/공지/사진 로드, 날짜 필터
- `chat` — 메시지 배열, 전송, 제안 질문
- `feature` — 행사별 기능 플래그
- `theme` — 7개 프리셋 테마 + 커스텀 색상 생성
- `popup` — 전역 팝업 (컴포넌트 동적 렌더링)
- `ui` — 모달 스택 관리 (중첩 모달 지원), 채팅 토글

### 라우터 가드 체계
- `requiresAuth` — JWT 인증 필수
- `requiresAdmin` — Admin 역할 필수
- `requiresConvention` — 행사 선택 필수 (localStorage `selectedConventionId`)
- 401 응답 시 자동 토큰 갱신 (failedQueue로 race condition 방지)

## 코딩 컨벤션

### 백엔드 (C#)
- **레이어**: Controller → Service → Repository (IUnitOfWork)
- **서비스 반환 패턴**: `Task<(bool Success, object Result, int StatusCode)>` — 컨트롤러에서 `StatusCode(statusCode, result)` 호출
- **DI**: 인터페이스 → 구현체, `ServiceCollectionExtensions.cs`의 도메인별 메서드에 등록
- **DTO 분리**: `DTOs/{Domain}Models/` 하위, 요청/응답 모델 분리
- **EF Core**: Code-First, IEntityTypeConfiguration은 `Data/Configurations/`
- **비동기**: `async/await` 사용, Async 접미사
- **상수**: `Constants/Roles.cs`, `Constants/DeleteStatus.cs`
- **유저 ID**: `ClaimsPrincipalExtensions.GetUserId()` / `GetUserIdOrNull()`
- **DeleteYn 패턴**: `"N"` = Active, `"Y"` = Deleted (문자열 기반, DB 마이그레이션 필요)
- **ScheduleTemplate/ScheduleItem/GuestScheduleTemplate**: `DTOs.ScheduleModels` 네임스페이스 (Entities 아님)
- **기존 경고**: CS8618(nullable), CS0618(obsolete ConfigJson) 등 46개 — 기존 것이므로 무시

### 프론트엔드 (Vue 3)
- **Composition API** (`<script setup>`) — Options API 사용 금지
- **JavaScript** — TypeScript 아님
- **`@` alias** → `ClientApp/src/`
- **API 호출**: `services/api.js`의 Axios 인스턴스 사용
- **Composable**: `composables/use*.js`
- **Tailwind CSS** 스타일링
- **모달**: `BaseModal.vue` (데스크탑 fade + 모바일 slide-up), `SlideUpModal.vue`
- **ESLint**: v9 flat config (`eslint.config.js`), `vue/multi-word-component-names: off`
- **Prettier**: Vue 템플릿에서 multiline @click 시 `<!-- prettier-ignore -->` 필요

## API 구조

### 주요 엔드포인트
| 구분 | Route | 인증 | 설명 |
|------|-------|------|------|
| 인증 | `/api/auth/*` | 불필요 | login, register, guest-login, refresh |
| 행사 | `/api/conventions/*` | JWT | 행사 CRUD |
| 관리자 | `/api/admin/*` | Admin | 참석자/일정/공지/SMS/통계/DB 관리 |
| 사용자 | `/api/users/*` | JWT | 프로필, 일정 |
| 채팅 | `/api/chat/*` | JWT | AI 채팅 |
| 파일 | `/api/files/*`, `/api/upload/*` | JWT | 파일 업로드/다운로드 |
| 공지 | `/api/notices/*` | JWT | 공지 CRUD |
| 설문 | `/api/surveys/*` | JWT | 설문 CRUD |
| 개인여행 | `/api/personal-trips/*` | JWT | 여행/항공/숙소/일정/체크리스트 |
| 시스템 | `/api/system/*` | Admin | 환경/DB/LLM 상태 |
| SignalR | `/chathub` | JWT (query param) | 실시간 채팅 |
| 헬스체크 | `/health` | 불필요 | LLM/벡터/임베딩/DB 상태 |

## 환경 설정

- `appsettings.json` / `appsettings.Production.json`: 백엔드 설정
- `ClientApp/.env.production`: 프론트엔드 환경변수 (`VITE_API_URL`)
- DB: SQL Server (`ConnectionStrings:DefaultConnection`, 60초 타임아웃, 3회 재시도)
- 파일 업로드: `StorageSettings:FileUploadPath`
- LLM: `LlmSettings` (gemini/llama3 전환)
- JWT: `JwtSettings` (Issuer, Audience, SecretKey, AccessToken만료분, RefreshToken만료일)
- CORS: `Cors:AllowedOrigins` (기본: localhost:3000)
- 임베딩: `EmbeddingSettings:UseOnnx` (ONNX 토글)

## 작업 순서 가이드

### 새 기능 추가 (풀스택)

```
1. 엔티티 설계
   → Entities/ 에 엔티티 클래스 생성
   → Data/Configurations/ 에 IEntityTypeConfiguration 추가
   → ConventionDbContext에 DbSet 추가
   → dotnet ef migrations add <Name>

2. Repository 등록
   → Repositories/IUnitOfWork.cs 에 IRepository<T> 프로퍼티 추가
   → Repositories/UnitOfWork.cs 에 lazy-initialized 백킹 필드 + 프로퍼티 구현

3. DTO 작성
   → DTOs/{Domain}Models/ 에 요청/응답 DTO 생성

4. Service 작성
   → Interfaces/I{Name}Service.cs 인터페이스 정의
   → Services/{Domain}/{Name}Service.cs 구현 (IUnitOfWork 주입)
   → Extensions/ServiceCollectionExtensions.cs 해당 도메인 메서드에 DI 등록

5. Controller 작성
   → Controllers/{Domain}/{Name}Controller.cs
   → [ApiController], [Route("api/...")], [Authorize] 설정
   → 서비스 메서드 호출, StatusCode(statusCode, result) 반환

6. 백엔드 빌드 검증
   → dotnet build --no-restore

7. 프론트엔드 API 연동
   → services/api.js 또는 별도 서비스 파일에 API 호출 함수 추가

8. Vue 컴포넌트 구현
   → components/{domain}/ 에 컴포넌트 생성
   → <script setup> + Composition API
   → 필요 시 라우터에 라우트 추가

9. 프론트엔드 빌드 검증
   → npm run build && npm run lint
```

### 기존 기능 수정

```
1. 영향 범위 파악
   → Controller 엔드포인트 확인
   → Service 메서드 확인
   → 프론트엔드 호출 지점 확인 (services/, components/)

2. 백엔드 수정
   → Service 로직 수정 (Controller는 최소한으로)
   → DTO 변경 필요 시 DTO 수정

3. 프론트엔드 수정
   → API 호출부 수정
   → 컴포넌트 UI 수정

4. 빌드 검증
   → dotnet build --no-restore
   → cd ClientApp && npm run build && npm run lint
```

### 컨트롤러에서 서비스 추출 (리팩토링)

```
1. Controller 비즈니스 로직 파악
2. Interfaces/I{Name}Service.cs 인터페이스 정의
3. Services/{Domain}/{Name}Service.cs 에 로직 이동 (IUnitOfWork 주입)
4. ServiceCollectionExtensions.cs에 DI 등록
5. Controller에서 서비스 호출로 교체
6. ConventionDbContext 직접 주입 제거
7. dotnet build --no-restore 검증
```

### DB 스키마 변경

```
1. Entity 수정/생성
2. Data/Configurations/ 에 설정 추가/수정
3. dotnet ef migrations add <MigrationName>
4. 마이그레이션 SQL 검토
5. dotnet ef database update
```

## 주의사항

- **프론트엔드는 JavaScript** (TypeScript 아님) — 타입 어노테이션 사용 X
- Vite 프록시: 개발 시 `/api` → `localhost:5000`
- 프로덕션: `ClientApp/dist` → `wwwroot` 복사, .NET에서 서빙
- `dotnet publish` 시 프론트엔드 자동 빌드 포함
- **dotnet run 중에는 dotnet build 파일 잠김** — 에러 시 CS 에러만 확인, MSB3027 무시
- **ConventionDbContext 직접 사용 허용**: DatabaseController(raw DB), DbSmsSender(ADO.NET 저장 프로시저), MigrationAnalyzer(마이그레이션 API) — 이 3개만
- **DeleteYn 문자열 패턴**: "N"/"Y" 또는 "1"/"2" 혼재 — DB 마이그레이션 필요 (미완료)
- **PersonalTripService** (893줄) — 분리 필요하나 미완료
- **한국어 주석** 일반적 — 깨진 인코딩 발견 시 수정
- **prettier vs Vue 컴파일러 충돌**: multiline `@click` 핸들러에 `<!-- prettier-ignore -->` + 세미콜론 결합 필요
