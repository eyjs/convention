# 🤝 스타투어 AI 챗봇 기여 가이드

스타투어 AI 챗봇 프로젝트에 기여해주셔서 감사합니다! 이 가이드는 프로젝트에 효과적으로 기여하는 방법을 설명합니다.

## 📋 기여 방법

### 🐛 버그 신고
- [GitHub Issues](https://github.com/yourusername/startour-ai-chatbot/issues)에서 새 이슈 생성
- 버그 신고 템플릿 사용
- 재현 가능한 단계 및 환경 정보 포함

### ✨ 기능 제안
- [GitHub Issues](https://github.com/yourusername/startour-ai-chatbot/issues)에서 기능 요청 이슈 생성
- 기능 요청 템플릿 사용
- 사용 사례 및 동기 명확히 설명

### 🔧 코드 기여
1. 저장소 Fork
2. Feature 브랜치 생성
3. 변경사항 구현
4. 테스트 추가/수정
5. Pull Request 생성

## 🏗️ 개발 환경 설정

### 전제 조건
```bash
- .NET 8 SDK
- Visual Studio 2022 또는 VS Code
- Git
- SQL Server LocalDB
```

### 로컬 설정
```bash
# 1. 저장소 클론
git clone https://github.com/yourusername/startour-ai-chatbot.git
cd startour-ai-chatbot

# 2. 의존성 설치
dotnet restore

# 3. 데이터베이스 설정
dotnet ef database update

# 4. 개발 서버 실행
dotnet watch run
```

## 📝 코딩 스타일 가이드

### C# 코딩 컨벤션
- [Microsoft C# 코딩 컨벤션](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) 준수
- PascalCase: 클래스, 메서드, 프로퍼티
- camelCase: 지역 변수, 매개변수
- 의미 있는 변수명 사용

```csharp
// ✅ 좋은 예
public class TravelService : ITravelService
{
    private readonly ILogger<TravelService> _logger;
    
    public async Task<TravelTrip> CreateTripAsync(CreateTripRequest request)
    {
        // 구현...
    }
}

// ❌ 나쁜 예
public class ts
{
    private ILogger l;
    
    public Task<TravelTrip> ct(object req)
    {
        // 구현...
    }
}
```

### API 설계 원칙
- RESTful 패턴 준수
- 명확한 HTTP 상태 코드 사용
- 일관된 JSON 응답 형식
- 적절한 에러 처리

```csharp
// ✅ 좋은 예
[HttpPost("trips")]
public async Task<ActionResult<TravelTrip>> CreateTrip([FromBody] CreateTripRequest request)
{
    try
    {
        var trip = await _travelService.CreateTripAsync(request);
        return CreatedAtAction(nameof(GetTrip), new { id = trip.Id }, trip);
    }
    catch (ValidationException ex)
    {
        return BadRequest(new { Error = ex.Message });
    }
}
```

## 🧪 테스트 가이드

### 테스트 작성 원칙
- 단위 테스트: 개별 메서드/클래스 테스트
- 통합 테스트: API 엔드포인트 테스트
- 테스트 커버리지 최소 80% 유지

### 테스트 실행
```bash
# 모든 테스트 실행
dotnet test

# 특정 프로젝트 테스트
dotnet test LocalRAG.Tests

# 커버리지 리포트 생성
dotnet test --collect:"XPlat Code Coverage"
```

### 테스트 예시
```csharp
[Test]
public async Task CreateTrip_ValidRequest_ReturnsCreatedTrip()
{
    // Arrange
    var request = new CreateTripRequest
    {
        TripName = "Test Trip",
        Destination = "Seoul"
    };

    // Act
    var result = await _controller.CreateTrip(request);

    // Assert
    Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
    var createdResult = (CreatedAtActionResult)result.Result;
    var trip = (TravelTrip)createdResult.Value;
    Assert.AreEqual(request.TripName, trip.TripName);
}
```

## 📖 문서화 가이드

### 코드 문서화
- 공개 API에 XML 문서 주석 추가
- 복잡한 로직에 인라인 주석 추가
- README.md 업데이트 (필요시)

```csharp
/// <summary>
/// 새로운 여행을 생성합니다.
/// </summary>
/// <param name="request">여행 생성 요청 정보</param>
/// <returns>생성된 여행 정보</returns>
/// <exception cref="ValidationException">요청 데이터가 유효하지 않은 경우</exception>
public async Task<TravelTrip> CreateTripAsync(CreateTripRequest request)
{
    // 구현...
}
```

## 🔄 Pull Request 프로세스

### PR 생성 전 체크리스트
- [ ] 기능이 완전히 구현됨
- [ ] 모든 테스트 통과
- [ ] 코드 스타일 준수
- [ ] 문서 업데이트 (필요시)
- [ ] 커밋 메시지가 명확함

### 커밋 메시지 형식
```
<type>(<scope>): <subject>

<body>

<footer>
```

**타입:**
- `feat`: 새로운 기능
- `fix`: 버그 수정
- `docs`: 문서만 변경
- `style`: 코드 의미에 영향을 주지 않는 변경사항
- `refactor`: 버그를 수정하거나 기능을 추가하지 않는 코드 변경
- `test`: 테스트 추가 또는 기존 테스트 수정
- `chore`: 빌드 프로세스 또는 보조 도구 변경

**예시:**
```
feat(travel): add trip creation API endpoint

- Add CreateTrip endpoint in TravelController
- Implement trip validation logic
- Add unit tests for trip creation

Closes #123
```

### PR 리뷰 과정
1. **자동 체크**: CI/CD 파이프라인 통과 확인
2. **코드 리뷰**: 최소 1명의 리뷰어 승인 필요
3. **테스트**: 기능 테스트 및 회귀 테스트
4. **병합**: Squash and merge 권장

## 🏷️ 라벨링 시스템

### 이슈 라벨
- `bug`: 버그 신고
- `enhancement`: 새로운 기능 요청
- `documentation`: 문서 관련
- `good first issue`: 초보자에게 적합한 이슈
- `help wanted`: 도움이 필요한 이슈
- `priority: high`: 높은 우선순위
- `priority: low`: 낮은 우선순위

### PR 라벨
- `work in progress`: 작업 중인 PR
- `ready for review`: 리뷰 준비 완료
- `needs changes`: 수정 필요
- `approved`: 승인됨

## 🎯 개발 우선순위

### 🔥 높은 우선순위
1. 보안 취약점 수정
2. 중요한 버그 수정
3. 성능 개선
4. API 안정성 향상

### 📋 보통 우선순위
1. 새로운 기능 추가
2. 코드 리팩토링
3. 테스트 커버리지 향상
4. 문서 개선

### 🔮 낮은 우선순위
1. UI/UX 개선
2. 코드 스타일 개선
3. 의존성 업데이트
4. 성능 최적화 (비중요)

## 🚀 릴리스 프로세스

### 버전 관리
- [Semantic Versioning](https://semver.org/) 준수
- `MAJOR.MINOR.PATCH` 형식
- 태그를 통한 릴리스 관리

### 릴리스 단계
1. **개발**: `develop` 브랜치에서 기능 개발
2. **테스트**: 통합 테스트 및 QA
3. **스테이징**: `main` 브랜치로 병합
4. **릴리스**: 태그 생성 및 배포

## 🤔 도움이 필요한가요?

### 연락처
- **GitHub Issues**: 기술적 질문 및 버그 신고
- **Discussions**: 일반적인 토론 및 아이디어 공유
- **Email**: technical.support@startour.com (가상)

### 유용한 리소스
- [.NET 8 문서](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core 가이드](https://docs.microsoft.com/en-us/ef/core/)
- [Ollama 문서](https://ollama.ai/docs)
- [Google Gemini API 문서](https://ai.google.dev/docs)

## 📄 라이선스

이 프로젝트에 기여함으로써 귀하의 기여가 프로젝트와 동일한 [MIT 라이선스](LICENSE) 하에 배포됨에 동의합니다.

---

**감사합니다!** 🙏

여러분의 기여로 스타투어 AI 챗봇이 더 나은 프로젝트가 됩니다!