# Convention Management System (LocalRAG)

행사(컨벤션) 관리 + RAG 기반 AI 챗봇 시스템. .NET 8 백엔드 + Vue 3 프론트엔드.

## 기술 스택

### 백엔드
- .NET 8 / ASP.NET Core (C#)
- Entity Framework Core 8 + SQL Server
- JWT 인증 (Bearer)
- SignalR (실시간 채팅)
- Serilog (로깅)
- EPPlus (엑셀 처리)
- ONNX Runtime (임베딩)

### 프론트엔드 (`ClientApp/`)
- Vue 3 (Composition API, JavaScript)
- Vite (빌드, 포트 3000)
- Pinia (상태 관리)
- Tailwind CSS
- Axios (HTTP)
- Vue Router
- Lucide (아이콘)

## 실행 명령어

```bash
# 백엔드
dotnet restore
dotnet run                    # http://localhost:5000

# 프론트엔드
cd ClientApp
npm install
npm run dev                   # http://localhost:3000 (API 프록시 -> :5000)

# 빌드
npm run build                 # ClientApp/dist 출력
dotnet publish                # 프론트엔드 자동 빌드 포함

# DB 마이그레이션
dotnet ef migrations add <Name>
dotnet ef database update

# 린트/포맷
cd ClientApp && npm run lint
cd ClientApp && npm run format
```

## 프로젝트 구조

```
/
├── Controllers/          # API 컨트롤러 (/api/*)
│   ├── Admin/            # 관리자 API
│   ├── Ai/               # AI/RAG 관련
│   ├── Auth/             # 인증 (로그인, 회원가입)
│   ├── Convention/       # 행사 CRUD
│   ├── Flight/           # 항공편 조회
│   ├── User/             # 사용자 관련
│   └── File/             # 파일 업로드
├── Entities/             # EF Core 엔티티 (DB 모델)
│   ├── Action/           # Dynamic Action 시스템
│   ├── Flight/           # 항공편
│   ├── Survey/           # 설문
│   └── *.cs              # Convention, User, UserConvention 등
├── DTOs/                 # 데이터 전송 객체
│   ├── AdminModels/
│   ├── ConventionModels/
│   └── ...
├── Services/             # 비즈니스 로직
│   ├── Ai/               # RAG, 임베딩, LLM
│   ├── Auth/             # 인증 서비스
│   ├── Chat/             # 채팅 서비스
│   ├── Convention/       # 행사 서비스
│   ├── Shared/           # 공통 (SMS, 인덱싱 등)
│   └── Upload/           # 파일/엑셀 업로드
├── Interfaces/           # 서비스 인터페이스 (I*.cs)
├── Repositories/         # 데이터 접근 레이어
├── Data/                 # DbContext
├── Middleware/            # 글로벌 예외, 요청 로깅
├── Providers/            # LLM 프로바이더 (Llama3, Gemini)
├── Hubs/                 # SignalR 허브 (ChatHub)
├── Migrations/           # EF Core 마이그레이션
├── Configuration/        # 설정 클래스
├── Storage/              # 벡터 스토어
├── ClientApp/            # Vue 3 프론트엔드
│   └── src/
│       ├── api/          # API 호출 모듈 (index.js)
│       ├── components/   # 재사용 컴포넌트
│       │   ├── admin/    # 관리자 컴포넌트
│       │   ├── chatbot/  # AI 챗봇
│       │   ├── common/   # 공통 UI
│       │   ├── notice/   # 공지
│       │   └── trip/     # 여행 관련
│       ├── views/        # 페이지 뷰
│       ├── stores/       # Pinia 스토어
│       ├── composables/  # Vue Composable
│       ├── router/       # 라우팅
│       ├── dynamic-features/ # Dynamic Action 렌더러
│       └── utils/        # 유틸리티
└── wwwroot/              # 정적 파일 (빌드 출력)
```

## 핵심 아키텍처

### 도메인 모델 (N:M)
- `User` ↔ `UserConvention` ↔ `Convention` (다대다 관계)
- `UserConvention`에 그룹명, 역할, AccessToken 등 컨텍스트 저장

### Dynamic Action System (핵심 기능)
- 관리자가 DB 설정만으로 프론트엔드 UI를 동적 주입
- `ConventionAction` → `ActionCategory`(BUTTON, BANNER, POPUP 등) + `TargetLocation` + `ConfigJson`
- `DynamicActionRenderer.vue`가 런타임에 컴포넌트 동적 렌더링
- `UserActionStatus`로 사용자별 상태 추적

### RAG AI 시스템
- `IEmbeddingService` → `MssqlVectorStore` → `LlmProviderManager` (Llama3/Gemini)
- SQL Server를 벡터 DB로 활용 (메모리 기반 유사도 계산)

## 코딩 컨벤션

### 백엔드 (C#)
- DI 패턴: 인터페이스 → 구현체, `Program.cs`에서 등록
- 서비스 레이어: `Interfaces/I*.cs` → `Services/**/*.cs`
- DTO 분리: `DTOs/{Domain}Models/` 하위에 요청/응답 모델
- EF Core: Code-First 마이그레이션
- 비동기 패턴: `async/await` 사용

### 프론트엔드 (Vue 3)
- Composition API (`<script setup>`)
- `@` alias → `ClientApp/src/`
- Pinia 스토어: `stores/*.js`
- API 호출: `api/index.js`에서 Axios 인스턴스 관리
- Composable: `composables/use*.js`
- Tailwind CSS로 스타일링

## 환경 설정

- `appsettings.json` / `appsettings.Production.json`: 백엔드 설정
- `ClientApp/.env.production`: 프론트엔드 환경변수
- DB: SQL Server (`ConnectionStrings:DefaultConnection`)
- 파일 업로드 경로: `StorageSettings:FileUploadPath`
- LLM: `LlmSettings` (gemini/llama3 전환 가능)

## API 구조

- 모든 API: `/api/*` 접두사
- 인증 필요 API: JWT Bearer 토큰
- SignalR Hub: `/chathub` (WebSocket)
- Health Check: `/health`
- Swagger: 개발 환경에서 `/swagger` 사용 가능

## 주의사항

- 프론트엔드는 JavaScript (TypeScript 아님)
- Vite 프록시 설정으로 개발 시 `/api` → `localhost:5000` 프록시
- 프로덕션 빌드 시 `ClientApp/dist` → `wwwroot`로 복사되어 .NET에서 서빙
- `dotnet publish` 시 프론트엔드 자동 빌드 포함
