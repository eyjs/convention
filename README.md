# Convention Management System (w/ RAG)

> .NET 8과 RAG(Retrieval-Augmented Generation)를 결합한 현대적인 행사 관리 및 AI 챗봇 백엔드 시스템

## 🚀 프로젝트 개요

본 시스템은 .NET 8 기반의 백엔드 API로, 행사(컨벤션) 관리 시스템과 RAG 기반 AI 챗봇 기능을 통합하여 제공합니다. 참석자는 여러 행사에 참여할 수 있으며(N:M), 행사별 일정, 공지사항 및 AI 챗봇을 통한 맞춤형 정보를 제공받습니다.

## 🏗️ 아키텍처
[Vue 3 Frontend] <---- (HTTPS/WSS) ----> [ASP.NET 8 API Backend] (ClientApp) | | | +--- (API) ---> [Controllers] | | | (Auth, Convention, Chat...) | | +--- (SignalR) -> [ChatHub] | | | +---- [Services] <------+---- [SQL Server DB] | (Auth, Notice, RAG...) | | | | +-- [Tables] +-- [RAG Service] | (Users, Conventions, | | | UserConventions, | +-- [MssqlVectorStore] -+ Schedules, Notices...) | | | | +-- [LlmProviderManager] | +-- [VectorDataEntry] | | | (Vector Table) | +-- [Llama3] | | +-- [Gemini] | | | +-- [EF Core DbContext] ----+

## 📋 주요 기능

* **행사 관리 시스템**: 행사(Convention) 생성, 조회, 수정, 삭제
* **참석자 관리 (N:M)**: 통합 `User` 모델 기반, `UserConvention`을 통한 다중 행사 참여 지원
* **일정 관리**: 행사별 일정 템플릿(`ScheduleTemplate`) 및 세부 항목 관리
* **공지사항 시스템**: 행사별 공지사항(`Notice`) 및 카테고리 관리
* **RAG AI 챗봇**:
    * **Vector DB**: 행사 정보(일정, 공지)를 `MssqlVectorStore`를 통해 SQL Server에 벡터화하여 저장
    * **다중 LLM 지원**: `LlmProviderManager`를 통해 Llama3, Gemini 등 DB 설정 기반 동적 LLM 선택
    * **지능형 Q&A**: 벡터 검색을 통한 컨텍스트 기반의 정확한 답변 생성
* **실시간 채팅**: `SignalR`을 이용한 행사별 실시간 그룹 채팅

## 🛠️ 기술 스택

### 백엔드 (Backend)
* **.NET 8**: 최신 .NET 플랫폼
* **ASP.NET Core**: RESTful API, SignalR
* **Entity Framework Core 8**: ORM 및 데이터베이스 관리
* **SQL Server**: 주 데이터베이스 및 벡터 저장소 (`MssqlVectorStore`)
* **Microsoft.ML.OnnxRuntime**: 로컬 임베딩 모델 실행
* **JWT (JSON Web Token)**: 인증 및 권한 부여

### 프론트엔드 (Frontend) - `ClientApp`
* **Vue 3**: Composition API
* **Vite**: 프론트엔드 빌드 도구
* **Pinia**: 상태 관리
* **Tailwind CSS**: 유틸리티 CSS 프레임워크
* **Axios / @microsoft/signalr**: API 및 WebSocket 클라이언트

## 🚀 설치 및 실행

### 1. 사전 요구사항
* .NET 8 SDK
* SQL Server (LocalDB 또는 정식 버전)
* Node.js (18.x 이상 권장)
* (선택) Ollama (로컬 Llama3 사용 시)

### 2. 백엔드 설정
```bash
# 리포지토리 클론
git clone <repository-url>
cd <repository-folder>

# NuGet 패키지 복원
dotnet restore

# appsettings.json의 ConnectionStrings 수정
# ...

# EF Core 데이터베이스 마이그레이션 적용
dotnet ef database update

# 백엔드 실행
dotnet run

# ClientApp 폴더로 이동
cd ClientApp

# npm 패키지 설치
npm install

# 개발 서버 실행 (Vite)
npm run dev

프론트엔드 (Vue): http://localhost:3000 (Vite 기본 포트)

백엔드 API (Swagger): https://localhost:7XXX 또는 http://localhost:5XXX (launchSettings.json 참조)

헬스 체크: /health

📚 API 엔드포인트 (예시)
POST /api/auth/login: 사용자 로그인

GET /api/users/me/conventions: 내가 참여 중인 행사 목록 조회

GET /api/conventions/{id}: 특정 행사 상세 정보 조회

GET /api/conventions/{id}/schedules: 행사 일정 조회

GET /api/conventions/{id}/notices: 행사 공지사항 목록 조회

POST /api/chat/query: RAG 챗봇 질의

GET /api/admin/conventions/{id}/users: (관리자) 행사별 참여자 목록 조회

POST /api/upload/users: (관리자) 엑셀로 참석자 일괄 업로드

/chathub: 실시간 채팅 SignalR 허브
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ConventionDb;Trusted_Connection=True;"
  },
  "LlmSettings": {
    "Provider": "Llama3", // DB의 LlmSetting 테이블에서 동적으로 관리됨 (이 설정은 무시될 수 있음)
    "Llama3": {
      "BaseUrl": "http://localhost:11434",
      "Model": "llama3"
    },
    "Gemini": {
      "ApiKey": "YOUR_API_KEY",
      "Model": "gemini-1.5-flash"
    }
  },
  "EmbeddingSettings": {
    "UseOnnx": false, // true로 설정 시 ONNX 모델 사용
    "ModelPath": "models/all-MiniLM-L6-v2.onnx"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SUPER_SECRET_KEY_REPLACE_IT",
    "Issuer": "ConventionApi",
    "Audience": "ConventionClient"
  }
}