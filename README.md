# Convention Management System (w/ RAG)

> .NET 8과 RAG(Retrieval-Augmented Generation)를 결합한 현대적인 행사 관리 및 AI 챗봇 백엔드 시스템

## 🚀 프로젝트 개요

본 시스템은 .NET 8 기반의 백엔드 API로, 행사(컨벤션) 관리 시스템과 RAG 기반 AI 챗봇 기능을 통합하여 제공합니다.

참석자는 여러 행사에 참여할 수 있으며(N:M), 행사별 일정, 공지사항 및 AI 챗봇을 통한 맞춤형 정보를 제공받습니다.

본 프로젝트의 가장 핵심적이고 복잡한 기능은 **Dynamic Action System**으로, 관리자가 API를 통해 프론트엔드(Vue)에 표시되는 UI 컴포넌트(버튼, 배너, 팝업 등)와 기능을 동적으로 주입, 제어 및 추적할 수 있습니다.

## 🏗️ 핵심 아키텍처

### 1. 도메인 모델 (N:M 관계)

참석자(`User`)가 여러 행사(`Convention`)에 참여할 수 있는 N:M 관계로 설계되었습니다.



* **`User` (회원/참석자):** 시스템의 핵심 사용자입니다. (`LoginId`, `PasswordHash` 등 고유 정보 소유)
* **`Convention` (행사):** 생성되는 개별 이벤트입니다.
* **`UserConvention` (행사 참여 정보):** `User`와 `Convention`을 잇는 매핑 테이블. 특정 행사에 참여하는 사용자의 맥락 정보(그룹명, 역할, `AccessToken` 등)를 저장합니다.

---

### 2. Dynamic Action System (핵심 기능)

본 시스템의 가장 강력한 기능으로, **백엔드(DB) 설정만으로 프론트엔드 UI/UX를 동적으로 렌더링**합니다. 이를 통해 코드 배포 없이도 행사별 맞춤형 기능(설문조사, 안내 팝업, 특정 메뉴 버튼)을 주입하고 사용자 반응을 추적할 수 있습니다.



**작동 방식:**
1.  **관리자 설정:** 관리자가 `ConventionAction` 엔티티를 생성합니다.
    * **`ActionCategory`**: 렌더링할 UI (e.g., `BUTTON`, `BANNER`, `AUTO_POPUP`, `CARD`)
    * **`TargetLocation`**: UI가 표시될 위치 (e.g., `HOME_SUB_HEADER`, `SCHEDULE_CONTENT_TOP`)
    * **`MapsTo`**: 클릭 시 연결될 기능 (e.g., `survey`, `travelInfo` 또는 특정 URL)
    * **`ConfigJson`**: UI에 필요한 세부 설정 (e.g., 버튼 텍스트, 배너 이미지 URL)
2.  **프론트엔드 렌더링:**
    * `ClientApp`이 `/api/conventions/{id}/actions`를 호출하여 현재 행사와 위치에 맞는 `ConventionAction` 목록을 수신합니다.
    * `DynamicActionRenderer.vue` 컴포넌트가 이 데이터를 기반으로 `GenericButton.vue`, `GenericBanner.vue` 등 실제 Vue 컴포넌트를 동적으로 렌더링합니다.
3.  **사용자 상호작용:**
    * 사용자가 동적 컴포넌트와 상호작용(클릭, 완료)합니다.
    * `UserActionStatus` 엔티티가 `UserId`와 `ConventionActionId`를 매핑하여 "설문 완료", "팝업 확인" 등의 사용자별 상태를 기록합니다.

---

### 3. RAG 및 AI 시스템

행사 정보를 기반으로 답변하는 AI 챗봇을 위해 RAG 파이프라인을 구축했습니다.

* **`IEmbeddingService`:** 텍스트를 벡터로 변환합니다. (ONNX/Local 지원)
* **`IVectorStore` (-> `MssqlVectorStore`):** SQL Server를 벡터 DB로 활용합니다.
    * `VectorDataEntry` 테이블에 벡터와 메타데이터(`conventionId` 등)를 저장합니다.
    * **[성능 참고]** 현재 `SearchAsync`는 DB에서 데이터를 메모리로 로드 후 유사도를 계산하므로, 데이터 증가 시 성능 개선이 필요할 수 있습니다.
* **`LlmProviderManager`:** DB(`LlmSetting` 테이블) 설정에 따라 `Llama3` 또는 `Gemini` 프로바이더를 동적으로 선택하여 제공합니다.

## 📋 주요 기능

* **행사 관리**: 행사(Convention) 생성, 조회, 수정, 삭제
* **참석자 관리 (N:M)**: 통합 `User` 모델 및 `UserConvention`을 통한 다중 행사 참여 지원
* **Dynamic Action System**: (핵심 기능) 백엔드 설정 기반의 동적 UI/기능 주입 및 사용자 상태 추적
* **RAG AI 챗봇**:
    * `MssqlVectorStore`를 통한 행사 정보(일정, 공지)의 벡터화
    * `LlmProviderManager`를 통한 다중 LLM(Llama3, Gemini) 동적 지원
* **일정 관리**: 행사별 일정 템플릿(`ScheduleTemplate`) 및 세부 항목(`ScheduleItem`) 관리
* **공지/게시판 시스템**: 행사별 공지(`Notice`) 및 카테고리(`NoticeCategory`) 관리
* **실시간 채팅**: `SignalR`을 이용한 행사별 실시간 그룹 채팅 (`ChatHub`)
* **엑셀 일괄 업로드**: 참석자, 일정, 속성 등 데이터 일괄 등록

## 🛠️ 기술 스택

### 백엔드 (Backend)
* **.NET 8** (LTS)
* **ASP.NET Core**: RESTful API, SignalR
* **Entity Framework Core 8**: ORM
* **SQL Server**: 주 데이터베이스 및 Vector Store
* **JWT (JSON Web Token)**: 인증 및 권한 부여
* **Microsoft.ML.OnnxRuntime**: 로컬 임베딩 모델 실행
* **Serilog**: 로깅
* **HealthChecks**: 시스템 모니터링

### 프론트엔드 (Frontend) - `ClientApp`
* **Vue 3** (Composition API)
* **Vite**: 프론트엔드 빌드 도구
* **Pinia**: 상태 관리
* **Tailwind CSS**: 유틸리티 UI 프레임워크
* **Axios**: HTTP 클라이언트
* **@microsoft/signalr**: 실시간 WebSocket 클라이언트

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

## 프론트엔드 설정 (별도 터미널)
# ClientApp 폴더로 이동
cd ClientApp

# npm 패키지 설치
npm install

# 개발 서버 실행 (Vite)
npm run dev
