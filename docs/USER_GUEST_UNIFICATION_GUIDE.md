# User와 Guest 엔티티 통합 마이그레이션 가이드

## 개요

이 문서는 `User`와 `Guest` 엔티티를 통합하는 데이터베이스 마이그레이션 가이드입니다. 이 리팩토링을 통해 시스템 사용자와 행사 참석자를 단일 `User` 엔티티로 통합하고, `UserConvention` 매핑 테이블을 통해 다대다 관계를 구현합니다.

## 변경 사항 요약

### 1. 엔티티 변경

#### 삭제된 엔티티
- `Guest` 엔티티 (Entities/Guest.cs) - 완전히 제거됨

#### 새로 생성된 엔티티
- `UserConvention` (Entities/UserConvention.cs) - User와 Convention 간의 다대다 매핑 테이블

#### 수정된 엔티티

**User (Entities/User.cs)**
- Guest의 프로필 속성 추가:
  - `CorpName` - 회사명
  - `CorpPart` - 부서
  - `ResidentNumber` - 주민등록번호
  - `Affiliation` - 소속
  - `EnglishName` - 영문 이름 (여권용)
  - `PassportNumber` - 여권 번호
  - `PassportExpiryDate` - 여권 만료일
- Navigation Properties 변경:
  - `ICollection<Guest> Guests` → `ICollection<UserConvention> UserConventions`
  - `ICollection<GuestAttribute> GuestAttributes` 추가
  - `ICollection<GuestActionStatus> GuestActionStatuses` 추가

**Convention (Entities/Convention.cs)**
- `ICollection<Guest> Guests` → `ICollection<UserConvention> UserConventions`

**GuestAttribute (Entities/GuestAttribute.cs)**
- `GuestId` → `UserId`로 FK 변경
- `Guest? Guest` → `User? User` Navigation Property 변경

**GuestScheduleTemplate (DTOs/ScheduleModels/ScheduleModels.cs)**
- `GuestId` → `UserId`로 FK 변경
- `Guest? Guest` → `User? User` Navigation Property 변경

**GuestActionStatus (Entities/GuestActionStatus.cs)**
- `GuestId` → `UserId`로 FK 변경
- `Guest Guest` → `User User` Navigation Property 변경

**ConventionChatMessage (Entities/ConventionChatMessage.cs)**
- `GuestId` → `UserId`로 FK 변경
- `Guest Guest` → `User User` Navigation Property 변경
- `GuestName` → `UserName`으로 필드명 변경

**SurveyResponse (Entities/SurveyResponse.cs)**
- `GuestId` → `UserId`로 FK 변경
- `Guest Guest` → `User User` Navigation Property 변경

### 2. DbContext 변경 (Data/ConventionDbContext.cs)

#### DbSet 변경
- `DbSet<Guest> Guests` 제거
- `DbSet<UserConvention> UserConventions` 추가

#### 엔티티 구성 변경

**User 구성**
```csharp
entity.HasIndex(e => e.Name).HasDatabaseName("IX_User_Name"); // 추가

entity.HasMany(u => u.UserConventions)
      .WithOne(uc => uc.User)
      .HasForeignKey(uc => uc.UserId)
      .OnDelete(DeleteBehavior.Cascade);

entity.HasMany(u => u.GuestAttributes)
      .WithOne(ga => ga.User)
      .HasForeignKey(ga => ga.UserId)
      .OnDelete(DeleteBehavior.Cascade);

entity.HasMany(u => u.GuestActionStatuses)
      .WithOne(gas => gas.User)
      .HasForeignKey(gas => gas.UserId)
      .OnDelete(DeleteBehavior.Cascade);
```

**Convention 구성**
```csharp
entity.HasMany(c => c.UserConventions)
      .WithOne(uc => uc.Convention)
      .HasForeignKey(uc => uc.ConventionId)
      .OnDelete(DeleteBehavior.Cascade);
```

**UserConvention 구성** (새로 추가)
```csharp
entity.HasKey(uc => new { uc.UserId, uc.ConventionId });
entity.Property(uc => uc.CreatedAt).HasDefaultValueSql("getdate()");

entity.HasIndex(e => e.UserId).HasDatabaseName("IX_UserConvention_UserId");
entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_UserConvention_ConventionId");
entity.HasIndex(e => e.AccessToken).IsUnique().HasDatabaseName("UQ_UserConvention_AccessToken");
entity.HasIndex(e => new { e.UserId, e.ConventionId })
      .IsUnique()
      .HasDatabaseName("UQ_UserConvention_UserId_ConventionId");
```

**Guest 구성** - 완전히 제거

**GuestAttribute 구성**
```csharp
entity.HasIndex(e => new { e.UserId, e.AttributeKey })  // GuestId → UserId
      .IsUnique()
      .HasDatabaseName("UQ_GuestAttributes_UserId_AttributeKey");

entity.HasOne(ga => ga.User)  // Guest → User
      .WithMany(u => u.GuestAttributes)
      .HasForeignKey(ga => ga.UserId)  // GuestId → UserId
      .OnDelete(DeleteBehavior.Cascade);
```

**GuestScheduleTemplate 구성**
```csharp
entity.HasKey(gst => new { gst.UserId, gst.ScheduleTemplateId });  // GuestId → UserId

entity.HasOne(gst => gst.User)  // Guest → User
      .WithMany()
      .HasForeignKey(gst => gst.UserId)  // GuestId → UserId
      .OnDelete(DeleteBehavior.Cascade);
```

**GuestActionStatus 구성**
```csharp
entity.HasIndex(e => e.UserId).HasDatabaseName("IX_GuestActionStatus_UserId");  // GuestId → UserId
entity.HasIndex(e => new { e.UserId, e.ConventionActionId })  // GuestId → UserId
      .IsUnique()
      .HasDatabaseName("UQ_GuestActionStatus_UserId_ConventionActionId");

entity.HasOne(e => e.User)  // Guest → User
      .WithMany(u => u.GuestActionStatuses)
      .HasForeignKey(e => e.UserId)  // GuestId → UserId
      .OnDelete(DeleteBehavior.NoAction);
```

**ConventionChatMessage 구성**
```csharp
entity.HasIndex(e => e.UserId).HasDatabaseName("IX_ChatMessage_UserId");  // 추가

entity.HasOne(e => e.User)  // Guest → User
      .WithMany()
      .HasForeignKey(e => e.UserId)  // GuestId → UserId
      .OnDelete(DeleteBehavior.NoAction);
```

**SurveyResponse 구성**
```csharp
entity.HasIndex(e => e.UserId).HasDatabaseName("IX_SurveyResponse_UserId");  // 추가

entity.HasOne(e => e.User)  // Guest → User
      .WithMany()
      .HasForeignKey(e => e.UserId)  // GuestId → UserId
      .OnDelete(DeleteBehavior.NoAction);
```

## 데이터 마이그레이션 전략

### Phase 1: 백업 및 준비

```bash
# 1. 현재 데이터베이스 백업
sqlcmd -S (localdb)\mssqllocaldb -d startour -Q "BACKUP DATABASE startour TO DISK='C:\Backups\startour_before_migration.bak'"

# 2. Git 커밋
git add .
git commit -m "Before User-Guest unification migration"
```

### Phase 2: 데이터 마이그레이션 스크립트

새로운 마이그레이션을 생성하기 전에, 기존 Guest 데이터를 User와 UserConvention으로 이관해야 합니다.

**마이그레이션 스크립트 생성**

```sql
-- Migrations/Scripts/MigrateGuestToUserConvention.sql

BEGIN TRANSACTION;

-- Step 1: Guest 테이블에 UserId가 NULL인 레코드에 대해 새 User 생성
INSERT INTO Users (LoginId, PasswordHash, Name, Email, Phone,
                   CorpName, CorpPart, ResidentNumber, Affiliation,
                   EnglishName, PassportNumber, PassportExpiryDate,
                   Role, IsActive, CreatedAt, UpdatedAt)
SELECT
    CONCAT('guest_', g.Id) as LoginId,  -- 임시 LoginId 생성
    ISNULL(g.PasswordHash, '') as PasswordHash,
    g.GuestName as Name,
    g.Email,
    g.Telephone as Phone,
    g.CorpName,
    g.CorpPart,
    g.ResidentNumber,
    g.Affiliation,
    g.EnglishName,
    g.PassportNumber,
    g.PassportExpiryDate,
    'Guest' as Role,
    1 as IsActive,
    GETDATE() as CreatedAt,
    GETDATE() as UpdatedAt
FROM Guests g
WHERE g.UserId IS NULL;

-- Step 2: Guest에 새로 생성된 UserId 업데이트
UPDATE g
SET g.UserId = u.Id
FROM Guests g
INNER JOIN Users u ON u.LoginId = CONCAT('guest_', g.Id)
WHERE g.UserId IS NULL;

-- Step 3: UserConvention 테이블 생성 및 데이터 이관
-- (마이그레이션에서 자동 생성되므로 여기서는 데이터만 이관)
INSERT INTO UserConventions (UserId, ConventionId, GroupName, AccessToken,
                             LastChatReadTimestamp, VisaDocumentAttachmentId, CreatedAt)
SELECT
    g.UserId,
    g.ConventionId,
    g.GroupName,
    g.AccessToken,
    g.LastChatReadTimestamp,
    g.VisaDocumentAttachmentId,
    GETDATE() as CreatedAt
FROM Guests g
WHERE g.UserId IS NOT NULL;

-- Step 4: GuestAttribute의 GuestId를 UserId로 매핑
-- (이미 Guest.UserId가 설정되어 있으므로 직접 업데이트)
UPDATE ga
SET ga.UserId = g.UserId
FROM GuestAttributes ga
INNER JOIN Guests g ON ga.GuestId = g.Id;

-- 기존 GuestId 컬럼 삭제는 마이그레이션에서 처리

-- Step 5: GuestScheduleTemplate의 GuestId를 UserId로 매핑
UPDATE gst
SET gst.UserId = g.UserId
FROM GuestScheduleTemplates gst
INNER JOIN Guests g ON gst.GuestId = g.Id;

-- Step 6: GuestActionStatus의 GuestId를 UserId로 매핑
UPDATE gas
SET gas.UserId = g.UserId
FROM GuestActionStatuses gas
INNER JOIN Guests g ON gas.GuestId = g.Id;

-- Step 7: ConventionChatMessage의 GuestId를 UserId로 매핑
UPDATE ccm
SET ccm.UserId = g.UserId,
    ccm.UserName = g.GuestName
FROM ConventionChatMessages ccm
INNER JOIN Guests g ON ccm.GuestId = g.Id;

-- Step 8: SurveyResponse의 GuestId를 UserId로 매핑
UPDATE sr
SET sr.UserId = g.UserId
FROM SurveyResponses sr
INNER JOIN Guests g ON sr.GuestId = g.Id;

COMMIT TRANSACTION;
```

### Phase 3: Entity Framework 마이그레이션 생성 및 적용

```bash
# 1. 새 마이그레이션 생성
dotnet ef migrations add UnifyUserAndGuest

# 2. 생성된 마이그레이션 파일 확인 및 수정
# - Migrations/YYYYMMDDHHMMSS_UnifyUserAndGuest.cs 파일 열기
# - Up() 메서드에서 테이블/컬럼 삭제 순서 확인
# - 필요시 데이터 이관 로직 추가

# 3. 마이그레이션 적용 전 테스트 (개발 환경)
dotnet ef migrations script > migration_preview.sql
# migration_preview.sql 검토

# 4. 마이그레이션 적용
dotnet ef database update
```

### Phase 4: 마이그레이션 파일 수정 (중요!)

생성된 마이그레이션 파일의 `Up()` 메서드에서 다음 순서로 작업이 진행되도록 수정:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. 새 UserConvention 테이블 생성
    migrationBuilder.CreateTable(
        name: "UserConventions",
        columns: table => new
        {
            UserId = table.Column<int>(nullable: false),
            ConventionId = table.Column<int>(nullable: false),
            // ... 기타 컬럼
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_UserConventions", x => new { x.UserId, x.ConventionId });
            // ... 기타 제약
        });

    // 2. User 테이블에 새 컬럼 추가
    migrationBuilder.AddColumn<string>(
        name: "CorpName",
        table: "Users",
        maxLength: 100,
        nullable: true);
    // ... 기타 컬럼 추가

    // 3. 관련 테이블에 UserId 컬럼 추가 (GuestId 대체용)
    migrationBuilder.AddColumn<int>(
        name: "UserId",
        table: "GuestAttributes",
        nullable: false,
        defaultValue: 0);
    // ... 기타 테이블도 동일하게

    // 4. 데이터 이관 SQL 실행
    migrationBuilder.Sql(@"
        -- 위의 MigrateGuestToUserConvention.sql 내용 삽입
    ");

    // 5. 외래 키 제거 (Guest 관련)
    migrationBuilder.DropForeignKey(
        name: "FK_GuestAttributes_Guests_GuestId",
        table: "GuestAttributes");
    // ... 기타 FK

    // 6. 인덱스 제거 (Guest 관련)
    migrationBuilder.DropIndex(
        name: "IX_GuestAttributes_GuestId",
        table: "GuestAttributes");
    // ... 기타 인덱스

    // 7. GuestId 컬럼 제거
    migrationBuilder.DropColumn(
        name: "GuestId",
        table: "GuestAttributes");
    // ... 기타 테이블

    // 8. Guests 테이블 제거
    migrationBuilder.DropTable(
        name: "Guests");

    // 9. 새 인덱스 및 외래 키 생성
    migrationBuilder.CreateIndex(
        name: "IX_GuestAttributes_UserId",
        table: "GuestAttributes",
        column: "UserId");
    // ... 기타 인덱스

    migrationBuilder.AddForeignKey(
        name: "FK_GuestAttributes_Users_UserId",
        table: "GuestAttributes",
        column: "UserId",
        principalTable: "Users",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);
    // ... 기타 FK
}
```

### Phase 5: 코드 리팩토링

마이그레이션 적용 후, 다음 코드들을 수정해야 합니다:

#### 수정이 필요한 주요 영역

1. **컨트롤러 (Controllers/)**
   - `Guest` 대신 `User` 및 `UserConvention` 사용
   - DTO 매핑 로직 수정
   - 예: `GuestController` → `UserConventionController` 또는 통합

2. **서비스 (Services/)**
   - `IGuestService` → 로직을 `IUserService` 및 `IUserConventionService`로 분산
   - Guest 관련 비즈니스 로직 리팩토링
   - 예: `Services/Guest/` → `Services/User/`, `Services/Convention/`

3. **리포지토리 (Repositories/)**
   - `IGuestRepository` 제거
   - `IUserConventionRepository` 추가
   - `IUnitOfWork`에서 Guests 제거, UserConventions 추가

4. **DTO (DTOs/)**
   - `GuestDto` → `UserDto` 및 `UserConventionDto`로 분리
   - 모든 DTO에서 `GuestId` → `UserId` 변경

5. **SignalR 허브 (Hubs/)**
   - `ChatHub`에서 Guest 대신 User 사용
   - 채팅 메시지 전송 시 UserId 사용

6. **프론트엔드 (ClientApp/)**
   - API 호출 경로 수정
   - Guest 관련 상태 관리 로직을 User로 변경
   - TypeScript 타입 정의 업데이트

#### 단계별 리팩토링 체크리스트

```bash
# 1. Guest 참조를 찾아서 수정
rg "Guest\b" --type cs | grep -v "GuestAttribute" | grep -v "GuestScheduleTemplate" | grep -v "GuestActionStatus"

# 2. GuestId 참조를 찾아서 UserId로 수정
rg "GuestId" --type cs

# 3. GuestName 참조를 찾아서 UserName으로 수정
rg "GuestName" --type cs

# 4. 빌드 에러 확인
dotnet build

# 5. 테스트 실행
dotnet test
```

## 롤백 계획

문제 발생 시 롤백 방법:

### 방법 1: 데이터베이스 백업 복원

```sql
-- SQL Server Management Studio 또는 sqlcmd 사용
USE master;
ALTER DATABASE startour SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE startour FROM DISK='C:\Backups\startour_before_migration.bak' WITH REPLACE;
ALTER DATABASE startour SET MULTI_USER;
```

### 방법 2: EF 마이그레이션 롤백

```bash
# 이전 마이그레이션으로 롤백
dotnet ef database update <PreviousMigrationName>

# Git 롤백
git revert <commit-hash>
```

## 검증 체크리스트

마이그레이션 후 다음 사항들을 검증하세요:

- [ ] 모든 기존 Guest 데이터가 User로 정확히 이관되었는가?
- [ ] UserConvention 테이블에 모든 행사-참석자 관계가 생성되었는가?
- [ ] GuestAttribute, GuestScheduleTemplate, GuestActionStatus의 UserId가 올바르게 매핑되었는가?
- [ ] ConventionChatMessage의 UserId와 UserName이 정확한가?
- [ ] SurveyResponse의 UserId가 올바르게 매핑되었는가?
- [ ] 모든 FK 제약 조건이 정상 작동하는가?
- [ ] 인덱스가 올바르게 생성되었는가?
- [ ] 애플리케이션 빌드가 성공하는가?
- [ ] 주요 기능(로그인, 행사 참석자 조회, 채팅 등)이 정상 작동하는가?

## 성능 고려 사항

- **UserConvention 복합 키**: (UserId, ConventionId) 복합 키를 사용하므로 조인 성능 고려
- **인덱스**: AccessToken, UserId, ConventionId에 인덱스 생성됨
- **쿼리 최적화**: User와 UserConvention 조인 시 `Include()` 사용 권장

```csharp
// Good: Eager loading
var user = await _context.Users
    .Include(u => u.UserConventions)
        .ThenInclude(uc => uc.Convention)
    .FirstOrDefaultAsync(u => u.Id == userId);

// Bad: N+1 query problem
var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
var conventions = user.UserConventions.Select(uc => uc.Convention).ToList(); // 추가 쿼리 발생
```

## 문의 및 지원

마이그레이션 중 문제가 발생하면:
1. 백업 확인
2. 에러 로그 수집 (logs/log.txt)
3. Git 커밋 히스토리 확인
4. 개발팀에 문의

---

**작성일:** 2025-10-31
**버전:** 1.0
**작성자:** Claude Code
