# EF Core Repository 패턴 구현 가이드

## 📚 목차
1. [개요](#개요)
2. [구조 설명](#구조-설명)
3. [설정 방법](#설정-방법)
4. [사용 예시](#사용-예시)
5. [모범 사례](#모범-사례)
6. [학습 포인트](#학습-포인트)

---

## 개요

이 프로젝트에는 **Repository 패턴**과 **Unit of Work 패턴**을 사용한 EF Core 구현체가 포함되어 있습니다.

### 왜 Repository 패턴을 사용하나요?

1. **관심사의 분리**: 데이터 액세스 로직을 비즈니스 로직에서 분리
2. **테스트 용이성**: Mock 객체로 쉽게 대체 가능
3. **재사용성**: 공통 CRUD 로직을 한 곳에서 관리
4. **유지보수성**: 데이터베이스 변경 시 Repository만 수정

---

## 구조 설명

### 📁 파일 구조

```
Repositories/
├── IRepository.cs                      # 제네릭 Repository 인터페이스
├── Repository.cs                       # 제네릭 Repository 구현체
├── IUnitOfWork.cs                      # Unit of Work 인터페이스 및 엔티티별 인터페이스
├── UnitOfWork.cs                       # Unit of Work 구현체
├── SpecificRepositories.cs             # 모든 엔티티별 Repository 구현체
├── ConventionRepository.cs             # Convention 특화 Repository
├── GuestRepository.cs                  # Guest 특화 Repository
└── RepositoryServiceExtensions.cs      # DI 등록 확장 메서드
```

### 🏗️ 계층 구조

```
Controller
    ↓
Service (비즈니스 로직)
    ↓
Unit of Work (트랜잭션 관리)
    ↓
Repository (데이터 액세스)
    ↓
DbContext (EF Core)
    ↓
Database
```

---

## 설정 방법

### 1. Program.cs 설정

```csharp
using Microsoft.Extensions.DependencyInjection;
using LocalRAG.Repositories;
using LocalRAG.Services;

var builder = WebApplication.CreateBuilder(args);

// 방법 1: 간단한 설정 (권장)
builder.Services.AddConventionDataAccess(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);

// 방법 2: 개별 설정
// builder.Services.AddDbContext<ConventionDbContext>(options =>
//     options.UseSqlServer(connectionString));
// builder.Services.AddRepositories();

// Service Layer 등록
builder.Services.AddScoped<ConventionService>();

// Controller 등록
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

### 2. appsettings.json 설정

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ConventionDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## 사용 예시

### 예시 1: 컨트롤러에서 직접 사용

```csharp
public class MyController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public MyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetConventions()
    {
        var conventions = await _unitOfWork.Conventions.GetActiveConventionsAsync();
        return Ok(conventions);
    }

    [HttpPost]
    public async Task<IActionResult> CreateConvention(Convention convention)
    {
        await _unitOfWork.Conventions.AddAsync(convention);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetConventions), new { id = convention.Id }, convention);
    }
}
```

### 예시 2: Service Layer 사용 (권장)

```csharp
// Service 클래스
public class ConventionService
{
    private readonly IUnitOfWork _unitOfWork;

    public ConventionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Convention> CreateConventionAsync(Convention convention)
    {
        // 비즈니스 로직
        convention.RegDtm = DateTime.Now;
        convention.DeleteYn = "N";

        await _unitOfWork.Conventions.AddAsync(convention);
        await _unitOfWork.SaveChangesAsync();

        return convention;
    }
}

// Controller에서 Service 사용
public class MyController : ControllerBase
{
    private readonly ConventionService _service;

    public MyController(ConventionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Convention convention)
    {
        var created = await _service.CreateConventionAsync(convention);
        return Ok(created);
    }
}
```

### 예시 3: 트랜잭션 사용

```csharp
public async Task RegisterGuestWithSchedules(Guest guest, List<int> scheduleIds)
{
    // 명시적 트랜잭션 시작
    await _unitOfWork.BeginTransactionAsync();

    try
    {
        // 1. 참석자 추가
        await _unitOfWork.Guests.AddAsync(guest);
        await _unitOfWork.SaveChangesAsync();

        // 2. 일정 등록
        foreach (var scheduleId in scheduleIds)
        {
            await _unitOfWork.GuestSchedules.AssignGuestToScheduleAsync(
                guest.Id, scheduleId);
        }
        await _unitOfWork.SaveChangesAsync();

        // 3. 커밋
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        // 실패 시 롤백
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### 예시 4: 페이징 처리

```csharp
public async Task<IActionResult> GetPagedConventions(int page = 1, int size = 10)
{
    var (items, totalCount) = await _unitOfWork.Conventions.GetPagedAsync(
        page,
        size,
        c => c.DeleteYn == "N"  // 조건
    );

    return Ok(new 
    {
        items,
        totalCount,
        totalPages = Math.Ceiling(totalCount / (double)size)
    });
}
```

### 예시 5: Include를 사용한 Eager Loading

```csharp
// Convention과 관련된 모든 데이터를 한 번에 로딩
var convention = await _unitOfWork.Conventions
    .GetConventionWithDetailsAsync(conventionId);

// convention.Guests, convention.Schedules 등이 모두 로딩됨
```

---

## 모범 사례

### ✅ DO (권장 사항)

1. **Service Layer 사용**
   ```csharp
   // ✅ 좋은 예
   public class ConventionService
   {
       private readonly IUnitOfWork _unitOfWork;
       
       public async Task CreateAsync(Convention convention)
       {
           // 비즈니스 로직을 Service에서 처리
           convention.RegDtm = DateTime.Now;
           await _unitOfWork.Conventions.AddAsync(convention);
           await _unitOfWork.SaveChangesAsync();
       }
   }
   ```

2. **트랜잭션 명확하게 관리**
   ```csharp
   // ✅ 좋은 예: 명시적 트랜잭션
   await _unitOfWork.BeginTransactionAsync();
   try
   {
       // 여러 작업...
       await _unitOfWork.CommitTransactionAsync();
   }
   catch
   {
       await _unitOfWork.RollbackTransactionAsync();
       throw;
   }
   ```

3. **AsNoTracking 활용**
   ```csharp
   // ✅ 읽기 전용 조회 시 성능 향상
   // Repository에서 이미 AsNoTracking() 사용 중
   var conventions = await _unitOfWork.Conventions.GetAllAsync();
   ```

4. **페이징 처리**
   ```csharp
   // ✅ 대량 데이터는 반드시 페이징
   var (items, total) = await _unitOfWork.Conventions
       .GetPagedAsync(page, size);
   ```

### ❌ DON'T (피해야 할 사항)

1. **DbContext를 직접 사용하지 마세요**
   ```csharp
   // ❌ 나쁜 예
   public class MyService
   {
       private readonly ConventionDbContext _context;
       
       public async Task DoSomething()
       {
           var data = await _context.Conventions.ToListAsync();
       }
   }
   
   // ✅ 좋은 예
   public class MyService
   {
       private readonly IUnitOfWork _unitOfWork;
       
       public async Task DoSomething()
       {
           var data = await _unitOfWork.Conventions.GetAllAsync();
       }
   }
   ```

2. **N+1 쿼리 문제 주의**
   ```csharp
   // ❌ 나쁜 예: N+1 쿼리 발생
   var conventions = await _unitOfWork.Conventions.GetAllAsync();
   foreach (var conv in conventions)
   {
       // 각 반복마다 DB 조회 발생!
       var guests = await _unitOfWork.Guests
           .GetGuestsByConventionIdAsync(conv.Id);
   }
   
   // ✅ 좋은 예: Eager Loading 사용
   var convention = await _unitOfWork.Conventions
       .GetConventionWithDetailsAsync(conventionId);
   // convention.Guests가 이미 로딩됨
   ```

3. **SaveChanges를 반복 호출하지 마세요**
   ```csharp
   // ❌ 나쁜 예
   foreach (var guest in guests)
   {
       await _unitOfWork.Guests.AddAsync(guest);
       await _unitOfWork.SaveChangesAsync(); // 반복마다 DB 접근!
   }
   
   // ✅ 좋은 예
   await _unitOfWork.Guests.AddRangeAsync(guests);
   await _unitOfWork.SaveChangesAsync(); // 한 번만 호출
   ```

---

## 학습 포인트

### 1. Repository 패턴의 핵심 개념

**Repository**는 데이터 저장소(Database)에 대한 추상화 레이어입니다.

```
비즈니스 로직 → Repository (인터페이스) → 실제 데이터 액세스
```

**장점:**
- 비즈니스 로직이 데이터베이스 구현에 의존하지 않음
- 테스트 시 Mock Repository로 대체 가능
- 데이터베이스 변경이 용이

### 2. Unit of Work 패턴

**Unit of Work**는 여러 Repository의 작업을 하나의 트랜잭션으로 묶습니다.

```csharp
// 하나의 비즈니스 트랜잭션
await _unitOfWork.Conventions.AddAsync(convention);
await _unitOfWork.Guests.AddAsync(guest);
await _unitOfWork.SaveChangesAsync(); // 모두 성공 또는 모두 실패
```

**핵심 원리:**
- 모든 Repository가 같은 DbContext 인스턴스 공유
- SaveChangesAsync 호출 전까지는 메모리에만 변경사항 존재
- 실패 시 자동 롤백 (트랜잭션)

### 3. EF Core Change Tracker

EF Core는 **Change Tracker**로 엔티티 상태를 추적합니다.

**엔티티 상태:**
- `Unchanged`: 변경 없음
- `Added`: 새로 추가됨 (INSERT)
- `Modified`: 수정됨 (UPDATE)
- `Deleted`: 삭제됨 (DELETE)
- `Detached`: 추적 안 함

```csharp
// AddAsync: EntityState를 Added로 변경
await _unitOfWork.Guests.AddAsync(guest);

// Update: EntityState를 Modified로 변경
_unitOfWork.Guests.Update(guest);

// Remove: EntityState를 Deleted로 변경
_unitOfWork.Guests.Remove(guest);

// SaveChangesAsync: Change Tracker의 변경사항을 SQL로 변환하여 실행
await _unitOfWork.SaveChangesAsync();
```

### 4. AsNoTracking vs Tracking

**AsNoTracking():**
- 읽기 전용 조회
- Change Tracker에 등록하지 않음
- **메모리 효율적, 성능 우수**
- 수정 불가

```csharp
// 읽기 전용: AsNoTracking 사용 (Repository에서 자동 적용)
var conventions = await _unitOfWork.Conventions.GetAllAsync();

// 수정할 예정: Tracking 필요
var convention = await _unitOfWork.Conventions.GetByIdAsync(id);
convention.Title = "수정";
_unitOfWork.Conventions.Update(convention);
await _unitOfWork.SaveChangesAsync();
```

### 5. Lazy Loading vs Eager Loading

**Lazy Loading:**
- 관련 데이터를 필요할 때 로딩
- 편리하지만 N+1 쿼리 문제 발생 가능

**Eager Loading (Include):**
- 관련 데이터를 미리 로딩
- 한 번의 쿼리로 모든 데이터 로딩
- **권장 방식**

```csharp
// Eager Loading 예시
public async Task<Convention?> GetConventionWithDetailsAsync(int id)
{
    return await _dbSet
        .Include(c => c.Guests)              // Guests 포함
            .ThenInclude(g => g.Attributes)  // Guest의 Attributes 포함
        .Include(c => c.Schedules)           // Schedules 포함
        .FirstOrDefaultAsync(c => c.Id == id);
}
```

### 6. 의존성 주입 생명주기

**Scoped (권장):**
- HTTP 요청당 하나의 인스턴스
- 같은 요청 내에서는 동일한 인스턴스 공유
- DbContext와 동일한 생명주기

```csharp
// Program.cs
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

**왜 Scoped인가?**
1. DbContext가 Scoped
2. 요청 내 트랜잭션 일관성 보장
3. 요청 종료 시 자동 Dispose

---

## 디버깅 팁

### 1. SQL 쿼리 로깅

```csharp
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### 2. Change Tracker 상태 확인

```csharp
// 디버깅 시 확인
var entries = _context.ChangeTracker.Entries();
foreach (var entry in entries)
{
    Console.WriteLine($"{entry.Entity.GetType().Name}: {entry.State}");
}
```

### 3. 일반적인 오류와 해결책

**오류 1: "The instance of entity type cannot be tracked..."**
```csharp
// 원인: 같은 ID의 엔티티가 이미 추적 중
// 해결: AsNoTracking 사용 또는 Detach
var entity = await _unitOfWork.Conventions.GetAsync(c => c.Id == id);
_context.Entry(entity).State = EntityState.Detached;
```

**오류 2: "SaveChanges 없이 데이터가 저장 안 됨"**
```csharp
// 원인: SaveChangesAsync 호출 누락
await _unitOfWork.Conventions.AddAsync(convention);
await _unitOfWork.SaveChangesAsync(); // ✅ 필수!
```

**오류 3: "Navigation property가 null"**
```csharp
// 원인: Include 누락
// 해결: GetConventionWithDetailsAsync 같은 Eager Loading 메서드 사용
var convention = await _unitOfWork.Conventions
    .GetConventionWithDetailsAsync(id);
```

---

## 추가 학습 자료

1. **EF Core 공식 문서**: https://learn.microsoft.com/ef/core/
2. **Repository 패턴**: https://martinfowler.com/eaaCatalog/repository.html
3. **Unit of Work 패턴**: https://martinfowler.com/eaaCatalog/unitOfWork.html

---

## 요약

### 핵심 개념 정리

1. **Repository**: 데이터 액세스 로직을 캡슐화
2. **Unit of Work**: 여러 Repository를 하나의 트랜잭션으로 관리
3. **Change Tracker**: EF Core의 엔티티 상태 추적 메커니즘
4. **Eager Loading**: Include로 관련 데이터를 미리 로딩
5. **DI Scoped**: HTTP 요청당 하나의 인스턴스

### 사용 순서

1. `Program.cs`에서 `AddConventionDataAccess()` 호출
2. Controller/Service에서 `IUnitOfWork` 주입받기
3. Repository 메서드로 데이터 조회/수정
4. `SaveChangesAsync()`로 변경사항 커밋

```csharp
// 전형적인 사용 패턴
public async Task<Convention> CreateConventionAsync(Convention convention)
{
    // 1. 비즈니스 로직
    convention.RegDtm = DateTime.Now;
    
    // 2. Repository를 통한 데이터 추가
    await _unitOfWork.Conventions.AddAsync(convention);
    
    // 3. Unit of Work로 커밋
    await _unitOfWork.SaveChangesAsync();
    
    return convention;
}
```
