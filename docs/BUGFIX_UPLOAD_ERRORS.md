# Bug Fix: Upload System Compilation Errors

## 발생한 오류들

### 1. `UpdateAsync` 메서드 없음
```
'IGuestRepository'에는 'UpdateAsync'에 대한 정의가 포함되어 있지 않습니다
```

**원인:** IRepository는 `Update(entity)` void 메서드만 제공 (EF Core Change Tracking 사용)

**수정:**
```csharp
// Before
await _unitOfWork.Guests.UpdateAsync(existingGuest);

// After
_unitOfWork.Guests.Update(existingGuest);
```

### 2. `ConventionAction.Config` 속성 없음
```
'ConventionAction'에는 'Config'에 대한 정의가 포함되어 있지 않습니다
```

**원인:** 실제 속성명은 `ConfigJson`

**수정:**
```csharp
// Before
Config = JsonSerializer.Serialize(...)

// After
ConfigJson = JsonSerializer.Serialize(...)
```

### 3. `ConventionAction.Description` 속성 없음
```
'ConventionAction'에는 'Description'에 대한 정의가 포함되어 있지 않습니다
```

**원인:** 엔티티에 Description 필드가 없었음

**수정:**
- `Entities/Action/ConventionAction.cs`에 `Description` 필드 추가
- `Data/ConventionDbContext.cs`에 설정 추가
- Migration SQL에 컬럼 추가 스크립트 포함

## 수정된 파일 목록

### 1. Services
- ✅ `Services/Upload/GuestUploadService.cs`
  - `UpdateAsync` → `Update`

- ✅ `Services/Upload/ScheduleTemplateUploadService.cs`
  - `Config` → `ConfigJson`
  - `Deadline` 필드 활용 (일정 시간 저장)

- ✅ `Services/Upload/AttributeUploadService.cs`
  - `UpdateAsync` → `Update`

### 2. Entities
- ✅ `Entities/Action/ConventionAction.cs`
  - `Description` 필드 추가 (nullable, NVARCHAR(4000))

### 3. Data
- ✅ `Data/ConventionDbContext.cs`
  - ConventionAction 설정에 `Description`, `ConfigJson` 추가

### 4. Migrations
- ✅ `Migrations/manual_add_groupname.sql`
  - ConventionActions에 Description 컬럼 추가 스크립트 포함

## Repository Update 패턴

### EF Core Change Tracking
```csharp
// ✅ 올바른 사용법
var entity = await _unitOfWork.Entities.GetByIdAsync(id);
if (entity != null)
{
    entity.SomeProperty = newValue;
    _unitOfWork.Entities.Update(entity); // Change Tracker에 등록
    await _unitOfWork.SaveChangesAsync(); // DB 저장
}

// ❌ 잘못된 사용법
await _unitOfWork.Entities.UpdateAsync(entity); // 이런 메서드 없음!
```

### Update vs UpdateAsync
- `Update(entity)`: void 메서드, Change Tracker에 수정 표시만 함
- `UpdateAsync`는 존재하지 않음
- 실제 DB 저장은 `SaveChangesAsync()` 호출 시 발생

## Migration 실행

### Entity Framework 방식
```bash
dotnet ef migrations add AddDescriptionToConventionAction
dotnet ef database update
```

### 수동 SQL 방식
```bash
# SQL Server Management Studio 또는 sqlcmd로 실행
sqlcmd -S (localdb)\mssqllocaldb -d startour -i Migrations/manual_add_groupname.sql
```

## 검증

### 컴파일 확인
```bash
dotnet build
```

### 예상 결과
- ✅ 컴파일 오류 0개
- ✅ 경고는 있을 수 있음 (nullable 관련)

## 주의사항

1. **SaveChangesAsync 호출 필수**
   - `Update()` 호출만으로는 DB에 저장되지 않음
   - 반드시 `await _unitOfWork.SaveChangesAsync()` 호출 필요

2. **트랜잭션 처리**
   - 모든 Upload 서비스는 트랜잭션으로 감싸져 있음
   - 오류 시 자동 롤백

3. **Change Tracking**
   - 같은 DbContext 내에서 조회한 엔티티만 추적됨
   - Detached 엔티티는 `Attach()` 후 `Update()` 필요

---

**수정 완료 일시:** 2025-10-24
**수정자:** Claude Code
