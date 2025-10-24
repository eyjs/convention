# Excel Upload System Refactoring

## 개요

기존의 모놀리식 Excel 업로드 시스템을 **참석자**, **일정 템플릿**, **속성** 업로드로 분리하고, **그룹 기반 일정 매핑** 기능을 추가한 리팩토링입니다.

## 변경사항 요약

### 기존 시스템 (Old)
- ✅ `ScheduleUploadService.UploadGuestsAndSchedulesAsync()`
  - 하나의 Excel 파일로 참석자 + 일정을 동시에 처리
  - 스케일링이 어렵고 유지보수가 복잡함

### 새로운 시스템 (New)
- ✅ **참석자 업로드** (`GuestUploadService`)
- ✅ **일정 템플릿 업로드** (`ScheduleTemplateUploadService`)
- ✅ **속성 업로드** (`AttributeUploadService`)
- ✅ **그룹-일정 매핑** (`GroupScheduleMappingService`)

---

## 1. 참석자 업로드 (Guest Upload)

### API Endpoint
```http
POST /api/upload/conventions/{conventionId}/guests
Content-Type: multipart/form-data
Authorization: Bearer {token}

file: [Excel 파일]
```

### Excel 형식
| 소속 | 부서 | 이름 | 사번(주민번호) | 전화번호 | 그룹 |
|------|------|------|---------------|----------|------|
| 본사 | 영업팀 | 홍길동 | 123456 | 010-1234-5678 | A그룹 |
| 지사 | 관리팀 | 김철수 | 234567 | 010-2345-6789 | B그룹 |

### 주요 기능
- ✅ 참석자 생성 또는 업데이트 (이름 + 전화번호로 unique 판단)
- ✅ **새로 추가된 필드:** `GroupName` (그룹 기반 일정 배정용)
- ✅ AccessToken 자동 생성 (비회원 접근용)

### Response
```json
{
  "success": true,
  "guestsCreated": 10,
  "guestsUpdated": 5,
  "totalProcessed": 15,
  "errors": [],
  "warnings": []
}
```

---

## 2. 일정 템플릿 업로드 (Schedule Template Upload)

### API Endpoint
```http
POST /api/upload/conventions/{conventionId}/schedule-templates
Content-Type: multipart/form-data
Authorization: Bearer {token}

file: [Excel 파일]
```

### Excel 형식
| 일정 헤더 | 상세 내용 |
|-----------|-----------|
| 11/03(일)_조식_07:30 | 호텔 1층 레스토랑<br/>뷔페 제공 |
| 11/03(일)_오프닝_09:00 | 대강당<br/>환영사 및 일정 안내 |

### 일정 헤더 형식
- **패턴:** `월/일(요일)_일정명_시:분`
- **예시:** `11/03(일)_조식_07:30`

### 상세 내용 처리
- **엑셀 내부 줄바꿈** (Shift+Enter): 자동으로 `<br/>` 태그로 변환
- 웹에서 올바르게 표시됨

### ConventionAction 생성
- `ActionType`: `SCHEDULE_0001`, `SCHEDULE_0002`, ... (자동 증가)
- `Title`: 일정명 (예: "조식", "오프닝")
- `Description`: 상세 내용 (HTML 형식)
- `MapsTo`: `"SCHEDULE"` (일정 타입)
- `Config`: JSON (날짜/시간 정보 저장)

### Response
```json
{
  "success": true,
  "templatesCreated": 8,
  "itemsCreated": 8,
  "errors": [],
  "warnings": [],
  "createdActions": [
    {
      "id": 123,
      "actionType": "SCHEDULE_0001",
      "title": "조식",
      "scheduleDateTime": "2025-11-03T07:30:00"
    }
  ]
}
```

---

## 3. 속성 업로드 (Attribute Upload)

### API Endpoint
```http
POST /api/upload/conventions/{conventionId}/attributes
Content-Type: multipart/form-data
Authorization: Bearer {token}

file: [Excel 파일]
```

### Excel 형식
| 이름 | 전화번호 | 나이 | 성별 | 직급 | 선호음식 | 알레르기 |
|------|----------|------|------|------|----------|----------|
| 홍길동 | 010-1234-5678 | 30 | 남 | 과장 | 한식 | 없음 |
| 김철수 | 010-2345-6789 | 35 | 남 | 부장 | 양식 | 새우 |

### 주요 기능
- ✅ 참석자의 메타 정보를 `GuestAttribute` 테이블에 저장
- ✅ **통계 정보 자동 생성**: 각 속성값별 분포 계산
- ✅ Upsert 방식: 기존 속성은 업데이트, 없으면 생성

### 통계 정보 활용
- 같은 속성값을 가진 참석자들을 그룹화
- 예: "한식을 선호하는 사람 15명", "알레르기가 있는 사람 3명"
- 행사 준비 시 유용한 데이터

### Response
```json
{
  "success": true,
  "attributesCreated": 50,
  "attributesUpdated": 10,
  "guestsProcessed": 10,
  "errors": [],
  "warnings": [],
  "statistics": {
    "나이": {
      "30": 5,
      "35": 3,
      "40": 2
    },
    "성별": {
      "남": 8,
      "여": 2
    },
    "선호음식": {
      "한식": 6,
      "양식": 3,
      "중식": 1
    }
  }
}
```

---

## 4. 그룹-일정 매핑 (Group-to-Schedule Mapping)

### 시나리오
1. **참석자 업로드**: 각 참석자에게 `GroupName` 부여 (예: "A그룹", "VIP")
2. **일정 템플릿 업로드**: 여러 일정 생성 (ConventionAction)
3. **그룹 매핑**: 특정 그룹의 모든 참석자에게 일정 일괄 배정

### API Endpoint
```http
POST /api/upload/schedule-mapping/group
Content-Type: application/json
Authorization: Bearer {token}

{
  "conventionId": 1,
  "guestGroup": "A그룹",
  "actionIds": [123, 124, 125, 126]
}
```

### 주요 기능
- ✅ 특정 그룹의 **모든 참석자**에게 여러 일정을 한 번에 배정
- ✅ `GuestActionStatus` 레코드 생성 (Guest ↔ ConventionAction)
- ✅ 중복 체크: 이미 매핑된 경우 건너뛰기
- ✅ 트랜잭션 처리: 전체 성공 또는 전체 롤백

### Response
```json
{
  "success": true,
  "guestsAffected": 25,
  "mappingsCreated": 100,
  "duplicatesSkipped": 5,
  "errors": []
}
```

### 그룹 목록 조회
```http
GET /api/upload/conventions/{conventionId}/groups
```

**Response:**
```json
["A그룹", "B그룹", "VIP", "일반참석자"]
```

---

## 5. 개별 일정 매핑 (기존 기능 유지)

### 기존 기능
- 관리자가 **특정 참석자**에게 **특정 일정**을 개별적으로 배정
- 그룹 매핑 후 예외 처리 또는 미세 조정에 사용

### 유지 사항
- 기존 API 엔드포인트 및 로직 변경 없음
- `GuestActionController` (또는 유사한 컨트롤러)에서 계속 사용 가능

---

## 데이터베이스 변경사항

### Guest 엔티티
```csharp
public class Guest
{
    // ... 기존 필드 ...

    /// <summary>
    /// 그룹명 (일정 템플릿 일괄 배정용)
    /// 예: "A그룹", "VIP", "일반참석자" 등
    /// </summary>
    [MaxLength(100)]
    public string? GroupName { get; set; }
}
```

### Migration 실행
```bash
# 방법 1: Entity Framework Migration
dotnet ef migrations add AddGuestGroupName
dotnet ef database update

# 방법 2: 수동 SQL 스크립트
# Migrations/manual_add_groupname.sql 실행
```

---

## API 사용 예시

### 전체 워크플로우

#### 1단계: 참석자 업로드
```bash
curl -X POST http://localhost:5000/api/upload/conventions/1/guests \
  -H "Authorization: Bearer {token}" \
  -F "file=@참석자업로드_샘플.xlsx"
```

#### 2단계: 일정 템플릿 업로드
```bash
curl -X POST http://localhost:5000/api/upload/conventions/1/schedule-templates \
  -H "Authorization: Bearer {token}" \
  -F "file=@일정업로드_샘플.xlsx"
```

#### 3단계: 속성 업로드 (선택사항)
```bash
curl -X POST http://localhost:5000/api/upload/conventions/1/attributes \
  -H "Authorization: Bearer {token}" \
  -F "file=@속성업로드_샘플.xlsx"
```

#### 4단계: 그룹별 일정 배정
```bash
# 그룹 목록 조회
curl http://localhost:5000/api/upload/conventions/1/groups \
  -H "Authorization: Bearer {token}"

# A그룹에 일정 배정
curl -X POST http://localhost:5000/api/upload/schedule-mapping/group \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "conventionId": 1,
    "guestGroup": "A그룹",
    "actionIds": [123, 124, 125, 126]
  }'
```

---

## 코드 구조

### 서비스 (Services/Upload/)
- `GuestUploadService.cs` - 참석자 업로드
- `ScheduleTemplateUploadService.cs` - 일정 템플릿 업로드
- `AttributeUploadService.cs` - 속성 업로드
- `GroupScheduleMappingService.cs` - 그룹-일정 매핑

### 인터페이스 (Interfaces/)
- `IGuestUploadService.cs`
- `IScheduleTemplateUploadService.cs`
- `IAttributeUploadService.cs`
- `IGroupScheduleMappingService.cs`

### DTOs (DTOs/UploadModels/)
- `GuestUploadResult.cs`
- `ScheduleTemplateUploadResult.cs`
- `AttributeUploadResult.cs`
- `GroupScheduleMappingRequest.cs`
- `GroupScheduleMappingResult.cs`

### 컨트롤러
- `Controllers/UploadController.cs` - 모든 업로드 API

---

## 마이그레이션 가이드

### 기존 코드 → 새 코드

#### 기존 (Old)
```csharp
var result = await _scheduleUploadService.UploadGuestsAndSchedulesAsync(
    conventionId,
    excelStream
);
```

#### 신규 (New)
```csharp
// 1. 참석자 업로드
var guestResult = await _guestUploadService.UploadGuestsAsync(
    conventionId,
    guestStream
);

// 2. 일정 템플릿 업로드
var scheduleResult = await _scheduleTemplateUploadService.UploadScheduleTemplatesAsync(
    conventionId,
    scheduleStream
);

// 3. 그룹별 일정 배정
var mappingResult = await _groupScheduleMappingService.MapGroupToSchedulesAsync(
    new GroupScheduleMappingRequest
    {
        ConventionId = conventionId,
        GuestGroup = "A그룹",
        ActionIds = new List<int> { 123, 124, 125 }
    }
);
```

---

## 장점

### 1. **관심사의 분리 (Separation of Concerns)**
- 각 업로드 기능이 독립적인 서비스로 분리
- 유지보수 및 테스트가 용이

### 2. **확장성 (Scalability)**
- 새로운 업로드 타입 추가가 쉬움
- 그룹 기반 매핑으로 대규모 행사 지원

### 3. **유연성 (Flexibility)**
- 참석자, 일정, 속성을 각각 독립적으로 관리
- 필요한 기능만 선택적으로 사용 가능

### 4. **통계 정보**
- 속성 업로드를 통한 참석자 분포 파악
- 행사 준비 및 운영에 유용한 인사이트

---

## 주의사항

1. **Excel 파일 형식**
   - `.xlsx` 형식만 지원 (`.xls` 불가)
   - 첫 번째 행은 헤더로 고정

2. **중복 처리**
   - 참석자: 이름 + 전화번호로 중복 판단
   - 일정 매핑: 중복 시 자동으로 건너뛰기

3. **트랜잭션**
   - 모든 업로드는 트랜잭션으로 처리
   - 오류 발생 시 전체 롤백

4. **권한**
   - 모든 API는 인증 필요 (`[Authorize]`)
   - 관리자 권한 확인 필요 시 추가 검증 구현

---

## 향후 개선사항

- [ ] 프론트엔드 UI 개발 (Vue 3)
- [ ] 업로드 진행률 표시 (SignalR)
- [ ] Excel 템플릿 다운로드 기능
- [ ] 업로드 히스토리 및 롤백 기능
- [ ] 대용량 파일 처리 최적화
- [ ] 에러 로그 상세화

---

## 샘플 파일

- `Sample/참석자업로드_샘플.xlsx`
- `Sample/일정업로드_샘플.xlsx`
- `Sample/속성업로드_샘플.xlsx`

---

**작성일**: 2025-10-24
**버전**: 1.0.0
