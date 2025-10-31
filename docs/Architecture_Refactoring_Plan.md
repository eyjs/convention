# 아키텍처 리팩토링 계획: Guest-Convention N:M 관계

## 목표
현재 Guest-Convention 1:1 관계를 N:M 관계로 변경하여, 한 참석자가 여러 행사에 참여할 수 있도록 개선

## 현재 구조 문제점

### 1. Guest 테이블 (문제)
```csharp
public class Guest
{
    public int ConventionId { get; set; } // ❌ 하나의 행사에만 종속
    public string GuestName { get; set; }
    public string Telephone { get; set; }
    public string? GroupName { get; set; } // 행사별 정보인데 Guest에 있음
    // ... 행사 독립적 정보 + 행사별 정보가 혼재
}
```

**문제:**
- 같은 사람이 2개 행사 참여 시 Guest 레코드 2개 생성 (데이터 중복)
- 전화번호 변경 시 모든 Guest 레코드 수정 필요
- 통합 프로필 관리 불가능

### 2. User 테이블 (불필요한 복잡도)
```csharp
public class User
{
    public string LoginId { get; set; } // AccessToken을 LoginId로 사용
    public ICollection<Guest> Guests { get; set; } // 1:N 관계
}
```

**문제:**
- LoginId가 AccessToken이라 사용자가 모름
- User-Guest 이중 구조로 로그인 로직 복잡 (3가지 방식)
- 모든 비즈니스 로직이 Guest 기반인데 User가 별도 존재

## 새로운 구조

### 엔티티 설계

```
┌─────────────────┐         ┌──────────────────────┐         ┌─────────────────┐
│     Guest       │         │  GuestConvention     │         │   Convention    │
├─────────────────┤         ├──────────────────────┤         ├─────────────────┤
│ Id (PK)         │────┐    │ Id (PK)              │    ┌────│ Id (PK)         │
│ Name            │    └───▶│ GuestId (FK)         │◀───┘    │ Title           │
│ Phone (Unique)  │         │ ConventionId (FK)    │         │ StartDate       │
│ Email           │         │ Role                 │         │ EndDate         │
│ PasswordHash    │         │ GroupName            │         │ ...             │
│ CreatedAt       │         │ AccessToken          │         └─────────────────┘
│ UpdatedAt       │         │ CorpName             │
└─────────────────┘         │ CorpPart             │
                            │ Affiliation          │
                            │ ResidentNumber       │
                            │ EnglishName          │
                            │ PassportNumber       │
                            │ PassportExpiryDate   │
                            │ LastChatReadTimestamp│
                            │ JoinedAt             │
                            │ IsActive             │
                            └──────────────────────┘
```

### 주요 변경사항

#### 1. Guest 테이블 (리팩토링)
**Before:**
- ConventionId (행사 종속)
- GuestName, Telephone, Email
- GroupName (행사별 정보)
- CorpName, CorpPart (행사별 정보)
- AccessToken (행사별 정보)
- IsRegisteredUser (User 연동)
- UserId (User FK)

**After:**
- Id
- Name (GuestName → Name)
- Phone (Telephone → Phone, Unique Index)
- Email
- PasswordHash (단일 비밀번호)
- CreatedAt, UpdatedAt
- ✅ 행사 독립적 정보만 저장

#### 2. GuestConvention 테이블 (신규)
**역할:**
- Guest-Convention N:M 매핑
- 행사별 참석자 정보 저장
- 행사별 권한 관리

**필드:**
- GuestId, ConventionId (복합 Unique Index)
- Role: "Participant" | "Admin" | "Owner"
- GroupName (행사별 그룹)
- AccessToken (행사별 액세스 토큰)
- CorpName, CorpPart, Affiliation (행사별 소속)
- ResidentNumber (행사별 주민번호)
- EnglishName, PassportNumber, PassportExpiryDate (해외 여행)
- LastChatReadTimestamp (행사별 채팅 읽음 시간)
- JoinedAt (참여 시간)
- IsActive (활성 여부)

#### 3. User 테이블 (제거)
**제거 이유:**
- 모든 비즈니스 로직이 Guest 기반
- Guest로 통합하여 단순화

## 마이그레이션 단계

### Step 1: 새 엔티티 생성
1. `Entities/GuestConvention.cs` 생성
2. `Guest.cs` 리팩토링 (ConventionId 제거, 필드 정리)

### Step 2: DbContext 업데이트
1. `DbSet<GuestConvention>` 추가
2. 관계 설정 (N:M)
3. Unique Index 설정 (GuestId + ConventionId)

### Step 3: 데이터 마이그레이션
```sql
-- 1. 기존 Guest → 새 Guest + GuestConvention 분리
INSERT INTO NewGuests (Name, Phone, Email, PasswordHash, CreatedAt)
SELECT DISTINCT GuestName, Telephone, Email,
       COALESCE(PasswordHash, '임시해시'), GETDATE()
FROM Guests
GROUP BY GuestName, Telephone, Email;

-- 2. GuestConvention 매핑 생성
INSERT INTO GuestConventions
(GuestId, ConventionId, Role, GroupName, AccessToken, ...)
SELECT ng.Id, og.ConventionId,
       CASE WHEN u.Role = 'Admin' THEN 'Admin' ELSE 'Participant' END,
       og.GroupName, og.AccessToken, ...
FROM Guests og
INNER JOIN NewGuests ng ON og.Telephone = ng.Phone
LEFT JOIN Users u ON og.UserId = u.Id;

-- 3. 외래키 업데이트 (GuestAttributes, GuestActionStatus 등)
-- 기존 GuestId → GuestConventionId로 변경 필요한 테이블들
```

### Step 4: 서비스 레이어 리팩토링
1. AuthService: 로그인 로직 단순화
2. GuestService: CRUD 로직 변경
3. ConventionService: 참석자 조회 로직 변경

### Step 5: API 엔드포인트 업데이트
```csharp
// Before
POST /api/auth/guest-login
{
  "name": "홍길동",
  "phone": "01012345678",
  "conventionId": 1
}

// After (동일하지만 내부 로직 변경)
POST /api/auth/login
{
  "phone": "01012345678",
  "password": "password123",
  "conventionId": 1 // optional, 지정 시 해당 행사로 자동 선택
}

// 새로운 엔드포인트
GET /api/me/conventions
→ 내가 참여중인 모든 행사 목록

POST /api/conventions/{id}/join
→ 기존 Guest가 새 행사 참여
```

### Step 6: 기존 테이블 제거
1. User 테이블 제거
2. 기존 Guest 테이블 → GuestOld로 백업 후 제거

## 예상 이점

### 1. 데이터 무결성
- ✅ 전화번호 변경 시 1개 레코드만 수정
- ✅ 중복 데이터 제거
- ✅ 정규화된 데이터 구조

### 2. 사용자 경험
- ✅ 한 번 로그인 → 모든 참여 행사 접근
- ✅ 통합 프로필 관리
- ✅ 행사 간 이동 간편

### 3. 확장성
- ✅ 행사별 권한 차별화 (A 행사는 참석자, B 행사는 관리자)
- ✅ 행사 초대 기능 추가 용이
- ✅ 참석 이력 추적 가능

### 4. 개발 편의성
- ✅ 로그인 로직 단순화
- ✅ User-Guest 이중 구조 제거
- ✅ 명확한 도메인 모델

## 위험 요소 및 대응

### 1. 외래키 의존성
**문제:** GuestAttributes, GuestActionStatus 등이 현재 Guest.Id 참조
**해결:**
- GuestConventionId로 FK 변경 (GuestId + ConventionId 조합)
- 또는 GuestConvention.Id를 새 FK로 사용

### 2. 기존 데이터 마이그레이션
**문제:** 동명이인 처리, 전화번호 중복
**해결:**
- 전화번호를 Unique Key로 사용
- 중복 시 수동 병합 필요 (관리자 툴 제공)

### 3. API 호환성
**문제:** 기존 프론트엔드 API 호출 영향
**해결:**
- 점진적 마이그레이션 (Adapter 패턴)
- API 버전 관리 (/api/v1, /api/v2)

## 타임라인

- [ ] Step 1-2: 엔티티 및 DbContext (1일)
- [ ] Step 3: 데이터 마이그레이션 스크립트 (1일)
- [ ] Step 4: 서비스 레이어 리팩토링 (2일)
- [ ] Step 5: API 엔드포인트 업데이트 (1일)
- [ ] Step 6: 테스트 및 배포 (1일)

**예상 소요 시간: 6일**

## 다음 단계

리팩토링 진행에 동의하시면:
1. GuestConvention 엔티티 구현
2. 마이그레이션 스크립트 작성
3. 단계별 실행

승인 후 작업 시작하겠습니다.
