# Program.cs 수정 가이드

## 📝 Repository 패턴을 추가하기 위한 수정사항

기존 Program.cs에 Repository 및 Unit of Work를 추가하려면 다음 단계를 따르세요.

---

## 1단계: Using 문 추가

**파일 상단에 추가:**

```csharp
using LocalRAG.Repositories;  // 추가
using LocalRAG.Services;
```

---

## 2단계: DbContext 설정 부분 수정

**기존 코드를 찾아서:**

```csharp
// Entity Framework with Smart Connection String
builder.Services.AddDbContext<ConventionDbContext>((serviceProvider, options) =>
{
    var connectionProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
    var connectionString = connectionProvider.GetConnectionString();
    
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
    });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
```

**다음과 같이 수정:**

```csharp
// Entity Framework with Smart Connection String + Repository Pattern
builder.Services.AddDbContext<ConventionDbContext>((serviceProvider, options) =>
{
    var connectionProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
    var connectionString = connectionProvider.GetConnectionString();
    
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
    });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Repository 및 Unit of Work 등록
builder.Services.AddRepositories();
```

---

## 3단계: Service 등록 (선택사항)

**기존 서비스 등록 부분 찾아서:**

```csharp
// Register core services
builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
builder.Services.AddScoped<IRagService, RagService>();
```

**아래에 ConventionService 추가:**

```csharp
// Register core services
builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
builder.Services.AddScoped<IRagService, RagService>();

// Convention 관련 서비스 (Repository 패턴 사용)
builder.Services.AddScoped<ConventionService>();
```

---

## 완성된 코드 (수정된 부분만)

```csharp
using LocalRAG.Data;
using LocalRAG.Repositories;  // ✅ 추가
using LocalRAG.Services;
// ... 기타 using 문들

var builder = WebApplication.CreateBuilder(args);

// ... 기존 설정들 ...

// Entity Framework with Smart Connection String
builder.Services.AddDbContext<ConventionDbContext>((serviceProvider, options) =>
{
    var connectionProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
    var connectionString = connectionProvider.GetConnectionString();
    
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
    });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ✅ Repository 및 Unit of Work 등록
builder.Services.AddRepositories();

// Register core services
builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
builder.Services.AddScoped<IRagService, RagService>();

// ✅ Convention 관련 서비스 (선택사항)
builder.Services.AddScoped<ConventionService>();

// ... 나머지 코드 ...
```

---

## 대안: 간단한 설정 방법

**모든 설정을 한 번에 하려면 기존 DbContext 설정 부분을 통째로 교체:**

```csharp
// 기존 코드 삭제:
// builder.Services.AddDbContext<ConventionDbContext>(...);

// 새로운 코드로 교체:
builder.Services.AddConventionDataAccess(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);

// 단, 이 경우 Smart Connection String Provider를 사용하려면 추가 설정 필요
```

---

## 검증 방법

**1. 빌드 확인:**
```bash
dotnet build
```

**2. 실행 확인:**
```bash
dotnet run
```

**3. DI 등록 확인:**
컨트롤러나 서비스에서 다음과 같이 주입받을 수 있어야 합니다:

```csharp
public class TestController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    
    public TestController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    [HttpGet("test-repository")]
    public async Task<IActionResult> Test()
    {
        var conventions = await _unitOfWork.Conventions.GetActiveConventionsAsync();
        return Ok(new { count = conventions.Count() });
    }
}
```

---

## 주의사항

1. **네임스페이스 확인**: `using LocalRAG.Repositories;` 추가 필수
2. **순서 중요**: DbContext 등록 → Repository 등록 순서 유지
3. **Service 등록**: ConventionService는 사용할 경우에만 등록
4. **Smart Connection String**: 기존 Smart Connection String Provider를 계속 사용하려면 `AddDbContext` 방식 유지

---

## 트러블슈팅

### 문제 1: "IUnitOfWork를 resolve할 수 없습니다"
```
해결: builder.Services.AddRepositories(); 추가 확인
```

### 문제 2: "순환 참조 오류"
```
해결: Repository 생성자에서 DbContext만 주입받도록 확인
```

### 문제 3: "DbContext가 Dispose 되었습니다"
```
해결: Repository를 Scoped로 등록했는지 확인 (기본값)
```

---

이제 Repository 패턴을 사용할 준비가 완료되었습니다! 🎉
