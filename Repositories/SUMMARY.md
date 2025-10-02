# EF Core Repository 패턴 구현 완료 요약

## ✅ 구현 완료 항목

### 📁 생성된 파일 목록

```
Repositories/
├── IRepository.cs                      ✅ 제네릭 Repository 인터페이스
├── Repository.cs                       ✅ 제네릭 Repository 구현체
├── IUnitOfWork.cs                      ✅ Unit of Work 및 엔티티별 인터페이스
├── UnitOfWork.cs                       ✅ Unit of Work 구현체
├── ConventionRepository.cs             ✅ Convention Repository 구현체
├── GuestRepository.cs                  ✅ Guest Repository 구현체
├── SpecificRepositories.cs             ✅ 나머지 9개 엔티티 Repository 구현체
├── RepositoryServiceExtensions.cs      ✅ DI 등록 확장 메서드
├── README.md                           ✅ 상세 가이드 문서
└── INSTALLATION.md                     ✅ 설치 가이드

Services/
└── ConventionService.cs                ✅ 서비스 레이어 예시 (10가지 패턴)

Controllers/
└── ConventionsExampleController.cs     ✅ API 컨트롤러 예시
```

---

## 🎯 구현된 기능

### 1. 제네릭 Repository (IRepository<T>)

**기본 CRUD 메서드:**
- `GetByIdAsync` - ID로 조회
- `GetAsync` - 조건으로 단일 조회
- `GetAllAsync` - 전체 조회
- `FindAsync` - 조건으로 다중 조회
- `GetPagedAsync` - 페이징 조회
- `ExistsAsync` - 존재 여부 확인
- `CountAsync` - 개수 조회
- `AddAsync` / `AddRangeAsync` - 추가
- `Update` / `UpdateRange` - 수정
- `Remove` / `RemoveRange` - 삭제
- `RemoveByIdAsync` - ID로 삭제

### 2. 엔티티별 특화 Repository (11개)

| Repository | 주요 특화 메서드 |
|-----------|----------------|
| **ConventionRepository** | GetConventionsByDateRangeAsync<br>GetConventionsByTypeAsync<br>GetConventionWithDetailsAsync<br>GetActiveConventionsAsync |
| **GuestRepository** | GetGuestsByConventionIdAsync<br>GetGuestWithDetailsAsync<br>SearchGuestsByNameAsync<br>GetGuestByCorpHrIdAsync |
| **ScheduleRepository** | GetSchedulesByConventionIdAsync<br>GetSchedulesByDateAsync<br>GetSchedulesByGroupAsync |
| **GuestAttributeRepository** | GetAttributesByGuestIdAsync<br>GetAttributeByKeyAsync<br>**UpsertAttributeAsync** |
| **CompanionRepository** | GetCompanionsByGuestIdAsync |
| **GuestScheduleRepository** | GetSchedulesByGuestIdAsync<br>GetGuestsByScheduleIdAsync<br>AssignGuestToScheduleAsync<br>RemoveGuestFromScheduleAsync |
| **FeatureRepository** | GetFeaturesByConventionIdAsync<br>GetEnabledFeaturesAsync<br>IsFeatureEnabledAsync |
| **MenuRepository** | GetMenusByConventionIdAsync<br>GetMenuWithSectionsAsync |
| **SectionRepository** | GetSectionsByMenuIdAsync |
| **OwnerRepository** | GetOwnersByConventionIdAsync |
| **VectorStoreRepository** | GetVectorsByConventionIdAsync<br>GetVectorsBySourceAsync |

### 3. Unit of Work 패턴

**트랜잭션 관리:**
- `SaveChangesAsync()` - 모든 변경사항 커밋
- `BeginTransactionAsync()` - 명시적 트랜잭션 시작
- `CommitTransactionAsync()` - 트랜잭션 커밋
- `RollbackTransactionAsync()` - 트랜잭션 롤백

**Repository 속성:**
```csharp
_unitOfWork.Conventions
_unitOfWork.Guests
_unitOfWork.Schedules
_unitOfWork.GuestAttributes
_unitOfWork.Companions
_unitOfWork.GuestSchedules
_unitOfWork.Features
_unitOfWork.Menus
_unitOfWork.Sections
_unitOfWork.Owners
_unitOfWork.VectorStores
```

---

## 📚 제공된 예시 코드

### Service Layer 예시 (ConventionService.cs)

10가지 실전 패턴 구현:

1. **단순 조회** - GetActiveConventionsAsync()
2. **단일 엔티티 생성** - CreateConventionAsync()
3. **복잡한 비즈니스 로직** - CreateConventionWithOwnersAsync()
4. **명시적 트랜잭션** - RegisterGuestWithSchedulesAsync()
5. **수정 작업** - UpdateConventionAsync()
6. **소프트 삭제** - SoftDeleteConventionAsync()
7. **페이징 처리** - GetConventionsPagedAsync()
8. **검색 기능** - SearchGuestsAsync()
9. **일괄 처리** - BulkRegisterGuestsAsync()
10. **복합 조회** - GetConventionStatisticsAsync()

### Controller 예시 (ConventionsExampleController.cs)

RESTful API 구현:
- `GET /api/conventions` - 활성 행사 목록
- `GET /api/conventions/{id}` - 행사 상세
- `GET /api/conventions/paged` - 페이징 조회
- `POST /api/conventions` - 행사 생성
- `POST /api/conventions/with-owners` - 담당자 포함 생성
- `PUT /api/conventions/{id}` - 행사 수정
- `DELETE /api/conventions/{id}` - 소프트 삭제
- `GET /api/conventions/{id}/statistics` - 통계 조회
- `GET /api/conventions/{conventionId}/guests/search` - 참석자 검색

---

## 🚀 사용 방법

### Step 1: Program.cs 수정

```csharp
using LocalRAG.Repositories;  // 추가

// DbContext 등록 후
builder.Services.AddRepositories();

// Service 등록 (선택)
builder.Services.AddScoped<ConventionService>();
```

### Step 2: Controller/Service에서 사용

```csharp
public class MyController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public MyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        var data = await _unitOfWork.Conventions.GetActiveConventionsAsync();
        return Ok(data);
    }
}
```

### Step 3: 트랜잭션 처리

```csharp
// 간단한 저장
await _unitOfWork.Conventions.AddAsync(convention);
await _unitOfWork.SaveChangesAsync();

// 복잡한 트랜잭션
await _unitOfWork.BeginTransactionAsync();
try
{
    await _unitOfWork.Conventions.AddAsync(convention);
    await _unitOfWork.SaveChangesAsync();
    
    await _unitOfWork.Guests.AddAsync(guest);
    await _unitOfWork.SaveChangesAsync();
    
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

---

## 🔑 핵심 개념 정리

### Repository 패턴

**목적**: 데이터 액세스 로직을 비즈니스 로직에서 분리

**장점**:
- ✅ 관심사의 분리
- ✅ 테스트 용이성 (Mock 사용 가능)
- ✅ 재사용성
- ✅ 유지보수성

**구조**:
```
Controller → Service → UnitOfWork → Repository → DbContext → Database
```

### Unit of Work 패턴

**목적**: 여러 Repository의 작업을 하나의 트랜잭션으로 관리

**핵심 원리**:
1. 모든 Repository가 같은 DbContext 인스턴스 공유
2. SaveChangesAsync() 호출 전까지는 메모리에만 변경사항 존재
3. 모두 성공하거나 모두 실패 (원자성)

### EF Core Change Tracker

**엔티티 상태**:
- `Unchanged` - 변경 없음
- `Added` - 추가됨 (INSERT)
- `Modified` - 수정됨 (UPDATE)
- `Deleted` - 삭제됨 (DELETE)
- `Detached` - 추적 안 함

**작동 방식**:
```csharp
await _unitOfWork.Guests.AddAsync(guest);     // EntityState.Added
_unitOfWork.Guests.Update(guest);             // EntityState.Modified
_unitOfWork.Guests.Remove(guest);             // EntityState.Deleted
await _unitOfWork.SaveChangesAsync();         // SQL 실행
```

---

## 💡 모범 사례

### ✅ DO (권장)

1. **Service Layer 사용**
   ```csharp
   // 비즈니스 로직은 Service에서
   public class ConventionService
   {
       private readonly IUnitOfWork _unitOfWork;
       
       public async Task CreateAsync(Convention conv)
       {
           conv.RegDtm = DateTime.Now;  // 비즈니스 로직
           await _unitOfWork.Conventions.AddAsync(conv);
           await _unitOfWork.SaveChangesAsync();
       }
   }
   ```

2. **페이징 처리**
   ```csharp
   // 대량 데이터는 반드시 페이징
   var (items, total) = await _unitOfWork.Conventions
       .GetPagedAsync(page, size);
   ```

3. **Eager Loading (Include)**
   ```csharp
   // N+1 쿼리 방지
   var conv = await _unitOfWork.Conventions
       .GetConventionWithDetailsAsync(id);
   ```

4. **일괄 처리**
   ```csharp
   // AddRangeAsync + SaveChangesAsync 한 번
   await _unitOfWork.Guests.AddRangeAsync(guests);
   await _unitOfWork.SaveChangesAsync();
   ```

### ❌ DON'T (피해야 할 사항)

1. **DbContext 직접 사용 금지**
   ```csharp
   // ❌ 나쁜 예
   var data = await _context.Conventions.ToListAsync();
   
   // ✅ 좋은 예
   var data = await _unitOfWork.Conventions.GetAllAsync();
   ```

2. **반복문에서 SaveChanges 금지**
   ```csharp
   // ❌ 나쁜 예
   foreach (var item in items)
   {
       await _unitOfWork.Guests.AddAsync(item);
       await _unitOfWork.SaveChangesAsync(); // 반복마다 DB 접근!
   }
   
   // ✅ 좋은 예
   await _unitOfWork.Guests.AddRangeAsync(items);
   await _unitOfWork.SaveChangesAsync(); // 한 번만
   ```

3. **N+1 쿼리 주의**
   ```csharp
   // ❌ 나쁜 예
   var convs = await _unitOfWork.Conventions.GetAllAsync();
   foreach (var c in convs)
   {
       var guests = await _unitOfWork.Guests
           .GetGuestsByConventionIdAsync(c.Id); // 반복마다 쿼리!
   }
   
   // ✅ 좋은 예
   var conv = await _unitOfWork.Conventions
       .GetConventionWithDetailsAsync(id); // Include 사용
   ```

---

## 📖 추가 학습 자료

### 제공된 문서
- `README.md` - 전체 가이드 (구조, 사용법, 학습 포인트)
- `INSTALLATION.md` - 설치 및 설정 가이드
- `ConventionService.cs` - 10가지 실전 패턴
- `ConventionsExampleController.cs` - API 구현 예시

### 외부 참고 자료
- [EF Core 공식 문서](https://learn.microsoft.com/ef/core/)
- [Repository 패턴](https://martinfowler.com/eaaCatalog/repository.html)
- [Unit of Work 패턴](https://martinfowler.com/eaaCatalog/unitOfWork.html)

---

## 🛠️ 다음 단계

### 1. 테스트 작성

```csharp
// Unit Test 예시
public class ConventionServiceTests
{
    [Fact]
    public async Task CreateConvention_ShouldSetDefaultValues()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var service = new ConventionService(mockUnitOfWork.Object);
        
        // Act
        var result = await service.CreateConventionAsync(new Convention
        {
            Title = "Test"
        });
        
        // Assert
        Assert.Equal("N", result.DeleteYn);
        Assert.NotEqual(default(DateTime), result.RegDtm);
    }
}
```

### 2. 추가 Repository 메서드

필요에 따라 특화 메서드 추가:

```csharp
public interface IConventionRepository : IRepository<Convention>
{
    // 커스텀 메서드 추가
    Task<IEnumerable<Convention>> GetUpcomingConventionsAsync();
    Task<Convention?> GetConventionByTitleAsync(string title);
}
```

### 3. 캐싱 추가

성능 향상을 위한 캐싱 레이어:

```csharp
public class CachedConventionRepository : IConventionRepository
{
    private readonly IConventionRepository _inner;
    private readonly IMemoryCache _cache;
    
    // 캐싱 로직 구현
}
```

---

## ✨ 요약

### 구현 완료
- ✅ 11개 엔티티에 대한 Repository 구현
- ✅ Unit of Work 패턴 적용
- ✅ DI 설정 완료
- ✅ Service Layer 예시 제공
- ✅ API Controller 예시 제공
- ✅ 상세 문서 작성

### 주요 이점
- 🎯 **관심사 분리**: 데이터 액세스 로직 캡슐화
- 🧪 **테스트 용이**: Mock 객체로 쉽게 테스트
- 🔄 **재사용성**: 공통 CRUD 로직 중앙화
- 🛡️ **트랜잭션 관리**: Unit of Work로 일관성 보장
- 📊 **성능 최적화**: AsNoTracking, Eager Loading 적용

### 사용 시작
1. `INSTALLATION.md` 참고하여 Program.cs 수정
2. Controller/Service에서 `IUnitOfWork` 주입
3. `ConventionService.cs` 예시 참고하여 비즈니스 로직 구현

---

**🎉 이제 Repository 패턴을 활용하여 효율적이고 유지보수 가능한 코드를 작성할 수 있습니다!**
