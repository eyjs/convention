# LocalRAG Travel Management System

> 여행 계획 관리와 RAG(Retrieval-Augmented Generation) 기능을 결합한 종합적인 백엔드 시스템

## 🚀 프로젝트 개요

LocalRAG Travel Management System은 .NET 8 기반의 현대적인 백엔드 API로, 다음과 같은 핵심 기능을 제공합니다:

- **여행 관리 시스템**: 여행 계획, 일정, 게스트 관리
- **RAG 시스템**: 벡터 데이터베이스를 활용한 지능형 질의응답
- **다중 LLM 지원**: Llama3, Gemini 등 다양한 LLM 프로바이더
- **실시간 임베딩**: 로컬/ONNX 기반 텍스트 임베딩
- **성능 모니터링**: 헬스 체크, 메트릭, 벤치마크

## 🏗️ 아키텍처

```
LocalRAG/
├── Controllers/           # API 엔드포인트
│   ├── TravelController   # 여행 관리 API
│   ├── RagController      # RAG 질의응답 API
│   ├── LlmController      # LLM 테스트 API
│   └── SystemController   # 시스템 모니터링 API
├── Data/                  # 데이터 레이어
│   ├── Entities/          # 데이터 모델
│   └── TravelDbContext    # EF Core DbContext
├── Services/              # 비즈니스 로직
│   ├── RagService         # RAG 핵심 서비스
│   ├── LocalEmbeddingService
│   └── OnnxEmbeddingService
├── Providers/             # LLM 프로바이더
│   ├── Llama3Provider
│   └── GeminiProvider
├── Storage/               # 벡터 스토리지
├── Middleware/            # 미들웨어
├── HealthChecks/          # 헬스 체크
└── wwwroot/               # 프론트엔드 UI
```

## 📋 주요 기능

### 1. 여행 관리 시스템
- ✅ **여행 CRUD**: 여행 계획 생성, 조회, 수정, 삭제
- ✅ **일정 관리**: 그룹별 세부 일정 관리
- ✅ **게스트 시스템**: 여행 참가자 관리
- ✅ **대시보드**: 여행별 통합 정보 제공

### 2. RAG (Retrieval-Augmented Generation)
- ✅ **벡터 데이터베이스**: 여행 정보의 벡터화 저장
- ✅ **의미 기반 검색**: 코사인 유사도 기반 문서 검색
- ✅ **컨텍스트 생성**: 검색된 문서 기반 답변 생성
- ✅ **실시간 동기화**: 여행 데이터의 벡터 DB 자동 동기화

### 3. LLM 프로바이더 시스템
- ✅ **Llama3 지원**: 로컬 Ollama 서버 연동
- ✅ **Gemini 지원**: Google Gemini API 연동
- ✅ **프로바이더 팩토리**: 설정 기반 동적 프로바이더 선택
- ✅ **테스트 인터페이스**: LLM 응답 품질 테스트

## 🛠️ 기술 스택

### 백엔드
- **.NET 8**: 최신 .NET 플랫폼
- **ASP.NET Core**: RESTful API 프레임워크
- **Entity Framework Core**: ORM 및 데이터베이스 관리
- **SQL Server**: 관계형 데이터베이스
- **Microsoft.ML.OnnxRuntime**: ML 모델 실행

### 프론트엔드
- **HTML5/CSS3/JavaScript**: 기본 웹 기술
- **Tailwind CSS**: 유틸리티 CSS 프레임워크
- **Font Awesome**: 아이콘 라이브러리

### AI/ML
- **Vector Database**: 인메모리 벡터 스토리지
- **Cosine Similarity**: 벡터 유사도 계산
- **Text Embedding**: 텍스트의 벡터 표현
- **RAG Pipeline**: 검색 증강 생성

## 🚀 설치 및 실행

### 1. 사전 요구사항
```bash
- .NET 8 SDK
- SQL Server (LocalDB 포함)
- Visual Studio 2022 또는 VS Code
- Ollama (Llama3 사용 시)
```

### 2. 프로젝트 설정
```bash
git clone <repository-url>
cd LocalRAG
dotnet restore
dotnet build
```

### 3. 데이터베이스 설정
```bash
dotnet ef database update
```

### 4. 실행
```bash
dotnet run
# 또는 개발 모드로
dotnet watch run
```

### 5. 웹 UI 접속
- **메인 페이지**: https://localhost:5001
- **Swagger API**: https://localhost:5001/swagger
- **헬스 체크**: https://localhost:5001/health

## 📚 API 엔드포인트

### 여행 관리 API
```http
GET    /api/travel/trips                    # 여행 목록 조회
POST   /api/travel/trips                    # 새 여행 생성
GET    /api/travel/trips/{id}               # 특정 여행 조회
POST   /api/travel/trips/{id}/schedules     # 일정 추가
POST   /api/travel/trips/{id}/sync-vector-data # 벡터 데이터 동기화
POST   /api/travel/chat                     # 여행 관련 채팅
```

### RAG API
```http
POST   /api/rag/documents                   # 문서 추가
POST   /api/rag/query                       # RAG 질의
GET    /api/rag/stats                       # RAG 통계
DELETE /api/rag/documents/{id}              # 문서 삭제
```

### LLM 테스트 API
```http
POST   /api/llm/test                        # LLM 응답 테스트
POST   /api/llm/test-embedding              # 임베딩 테스트
GET    /api/llm/providers                   # 사용 가능한 프로바이더
```

### 시스템 모니터링 API
```http
GET    /api/system/info                     # 시스템 정보
GET    /api/system/metrics                  # 성능 메트릭
POST   /api/system/benchmark                # 벤치마크 실행
```

## ⚙️ 설정 옵션

### appsettings.json 주요 설정
```json
{
  "LlmSettings": {
    "Provider": "Llama3",
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
    "UseOnnx": false,
    "ModelPath": "models/all-MiniLM-L6-v2.onnx",
    "Dimensions": 384
  }
}
```

## 🔧 개발자 가이드

### 새로운 LLM 프로바이더 추가

1. `ILlmProvider` 인터페이스 구현
2. `Providers` 폴더에 새 프로바이더 클래스 생성
3. `Program.cs`에서 DI 컨테이너에 등록
4. `appsettings.json`에 설정 추가

```csharp
public class CustomLlmProvider : ILlmProvider
{
    public string ProviderName => "Custom";
    
    public async Task<string> GenerateResponseAsync(string prompt, string? context = null)
    {
        // 구현
    }
    
    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        // 구현
    }
}
```

## 🧪 테스트

### 단위 테스트 실행
```bash
dotnet test
```

### 통합 테스트
```bash
# 시스템이 실행 중인 상태에서
curl https://localhost:5001/health
```

### 성능 벤치마크
```bash
# 웹 UI 또는 API를 통해
POST /api/system/benchmark
```

## 📊 모니터링 및 로깅

### 헬스 체크 엔드포인트
- **전체 상태**: `/health`
- **개별 컴포넌트**: 각 헬스 체크 클래스에서 세부 상태 확인

### 로그 레벨 설정
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "LocalRAG": "Debug"
    }
  }
}
```

## 🚀 프로덕션 배포

### Docker 배포 (예정)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
EXPOSE 80
ENTRYPOINT ["dotnet", "LocalRAG.dll"]
```

### 환경 변수
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="Server=...;"
LlmSettings__Gemini__ApiKey="your-key"
```

## 🔍 문제 해결

### 자주 발생하는 문제

1. **Ollama 연결 실패**
   ```bash
   # Ollama 서비스 상태 확인
   ollama list
   ollama serve
   ```

2. **데이터베이스 연결 오류**
   ```bash
   # 마이그레이션 적용
   dotnet ef database update
   ```

3. **임베딩 모델 로드 실패**
   - `models/` 폴더에 ONNX 모델 파일 확인
   - `EmbeddingSettings:UseOnnx`를 `false`로 설정하여 기본 임베딩 사용

## 🤝 기여하기

1. 이슈 리포트
2. 기능 제안
3. Pull Request
4. 문서 개선

### 개발 환경 설정
```bash
git clone <repository>
cd LocalRAG
dotnet restore
dotnet build
dotnet test
```

## 📄 라이선스

MIT License - 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

## 🙏 감사의 말

- **Microsoft .NET Team**: .NET 8 및 ASP.NET Core
- **Ollama Team**: 로컬 LLM 실행 환경
- **Google**: Gemini API
- **Tailwind CSS**: UI 스타일링

---

**Happy Coding! 🚀**
