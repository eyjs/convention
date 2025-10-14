# 🎯 참석자 속성 일괄 매핑 기능 - 완료 가이드

## 📋 구현 완료 내역

### ✅ 백엔드 (C#)

#### 1. 새로 생성된 파일
- `Models/DTOs/GuestAttributeDtos.cs` ✅

#### 2. 수정된 파일
- `Controllers/GuestController.cs` ✅
  - `BulkAssignAttributes()` 메서드 추가
  - `GetParticipantsWithAttributes()` 메서드 추가

### ✅ 프론트엔드 (Vue.js)

#### 1. 새로 생성된 파일
- `ClientApp/src/views/BulkAssignAttributes.vue` ✅

#### 2. 수정된 파일
- `ClientApp/src/router/index.js` ✅
  - `/admin/bulk-assign` 라우트 추가

---

## 🚀 실행 방법

### 1. 백엔드 실행

```bash
cd C:\Users\USER\dev\startour\convention
dotnet run
```

또는 Visual Studio에서 F5

### 2. 프론트엔드 실행 (개발 모드)

```bash
cd C:\Users\USER\dev\startour\convention\ClientApp
npm run dev
```

### 3. 접속

브라우저에서 다음 URL로 접속:
```
http://localhost:5173/admin/bulk-assign
```

---

## 📝 사용 방법

### Step 1: 행사 선택
1. 페이지 상단 드롭다운에서 행사 선택
2. 자동으로 속성 정의와 참석자 목록 로드

### Step 2: 참석자 선택
- **개별 선택**: 카드 클릭 또는 체크박스 선택
- **전체 선택**: "전체 선택" 버튼 클릭
- **검색 후 선택**: 검색창에서 필터링 후 선택

### Step 3: 속성 설정
1. "선택한 참석자 속성 설정" 버튼 클릭
2. 모달에서 속성 입력
3. 미리보기 확인
4. "일괄 저장" 클릭

---

## 🎯 주요 API 엔드포인트

### POST `/api/guest/bulk-assign-attributes`
참석자 다중 속성 일괄 할당 (Admin 전용)

**요청 예시**:
```json
{
  "conventionId": 1,
  "guestMappings": [
    {
      "guestId": 101,
      "attributes": {
        "버스": "1호차",
        "티셔츠": "L"
      }
    }
  ]
}
```

**응답 예시**:
```json
{
  "success": true,
  "message": "50명의 참석자에게 속성이 성공적으로 할당되었습니다.",
  "totalProcessed": 50,
  "successCount": 50,
  "failCount": 0,
  "errors": []
}
```

### GET `/api/guest/participants-with-attributes`
속성 포함 참석자 목록 조회 (Admin 전용)

**쿼리 파라미터**: `conventionId`

**응답 예시**:
```json
[
  {
    "id": 101,
    "guestName": "홍길동",
    "corpName": "스타투어",
    "corpPart": "개발팀",
    "telephone": "010-1234-5678",
    "email": "hong@example.com",
    "currentAttributes": {
      "버스": "1호차",
      "티셔츠": "L"
    }
  }
]
```

---

## 🔍 테스트 시나리오

### 시나리오 1: 버스 호차 일괄 배정
1. 행사 선택
2. 50명 참석자 선택
3. "버스" 속성에 "1호차" 선택
4. 저장
5. ✅ 50명 모두 버스 속성 "1호차"로 설정 확인

### 시나리오 2: 회사별 티셔츠 배정
1. 검색창에 회사명 입력
2. "전체 선택" 클릭
3. "티셔츠" 속성에 사이즈 선택
4. 저장
5. ✅ 해당 회사 직원 모두 티셔츠 사이즈 설정 확인

### 시나리오 3: 다중 속성 동시 배정
1. VIP 20명 선택
2. 속성 설정 모달에서:
   - 버스: "1호차"
   - 티셔츠: "XL"
   - 특별등급: "VIP"
3. 저장
4. ✅ 20명에게 3개 속성 모두 설정 확인

---

## 💡 핵심 기능

### 1. 트랜잭션 기반 처리
- 데이터 일관성 보장
- 오류 발생 시 자동 롤백

### 2. 기존 속성 보존
- 새로 설정하는 키만 업데이트
- 다른 속성은 유지

### 3. 실시간 검색
- 이름, 회사, 부서로 실시간 필터링

### 4. 미리보기
- 저장 전 설정 내용 확인
- 처음 5명의 참석자 미리보기

### 5. 반응형 UI
- 모바일/태블릿 대응
- 그리드 자동 조정

---

## 🐛 트러블슈팅

### 문제: "401 Unauthorized"
**원인**: 인증 토큰이 없거나 만료됨  
**해결**: Admin 계정으로 다시 로그인

### 문제: "403 Forbidden"
**원인**: Admin 권한이 없음  
**해결**: 
```sql
-- 사용자 Role 확인
SELECT * FROM Users WHERE Username = 'your_username';

-- Role 업데이트
UPDATE Users SET Role = 'Admin' WHERE Username = 'your_username';
```

### 문제: 참석자 목록이 안 보임
**원인**: API 요청 실패 또는 데이터 없음  
**해결**: 
1. F12 > Network 탭에서 API 응답 확인
2. 데이터베이스 확인:
```sql
SELECT COUNT(*) FROM Guests WHERE ConventionId = 1;
```

### 문제: 속성 정의가 안 보임
**원인**: AttributeDefinitions 테이블에 데이터 없음  
**해결**: 
```sql
-- 속성 정의 추가
INSERT INTO AttributeDefinitions 
  (ConventionId, AttributeKey, Options, OrderNum, IsRequired)
VALUES 
  (1, '버스', '["1호차","2호차","3호차"]', 1, 1),
  (1, '티셔츠', '["S","M","L","XL"]', 2, 0);
```

### 문제: CORS 오류
**원인**: 백엔드에서 프론트엔드 Origin을 허용하지 않음  
**해결**: `Program.cs`에서 CORS 정책 확인
```csharp
app.UseCors("AllowVueApp");
```

---

## 📊 데이터베이스 관계

```
Conventions (행사)
    |
    +-- AttributeDefinitions (속성 정의)
    |       - AttributeKey: "버스"
    |       - Options: ["1호차","2호차","3호차"]
    |
    +-- Guests (참석자)
            |
            +-- GuestAttributes (참석자 속성)
                    - AttributeKey: "버스"
                    - AttributeValue: "1호차"
```

---

## 🎨 UI 구성요소

### 1. 헤더
- 제목: "🎯 참석자 속성 일괄 매핑"
- 부제목: "여러 참석자에게 속성을 한번에 설정할 수 있습니다"

### 2. 템플릿 섹션
- 행사 선택 드롭다운
- 설정 가능한 속성 목록 (칩 형태)

### 3. 컨트롤 바
- 전체 선택/해제 버튼
- 선택된 참석자 수 표시
- 검색 입력창
- 일괄 설정 버튼

### 4. 참석자 그리드
- 카드 형식으로 표시
- 체크박스로 선택
- 현재 속성 표시
- 반응형 그리드 (350px 최소 너비)

### 5. 일괄 설정 모달
- 속성 입력 폼
- 미리보기 섹션
- 저장/취소 버튼

### 6. 토스트 알림
- 성공: 초록색
- 오류: 빨간색
- 경고: 주황색

---

## ✅ 완료 체크리스트

- [x] DTO 생성
- [x] Controller 메서드 추가
- [x] Vue 컴포넌트 생성
- [x] 라우터 설정
- [x] API 통합
- [x] UI/UX 구현
- [x] 트랜잭션 처리
- [x] 에러 핸들링
- [x] 반응형 디자인

---

## 📚 코드 구조

### 백엔드 주요 로직

**BulkAssignAttributes 메서드**:
```csharp
1. 트랜잭션 시작
2. 참석자 ID 검증
3. 기존 속성 중 겹치는 키만 삭제
4. 새 속성 추가 (빈 값 제외)
5. 커밋 또는 롤백
```

**특징**:
- 부분 업데이트 지원 (기존 속성 보존)
- 일괄 처리로 성능 최적화
- 상세한 오류 정보 제공

### 프론트엔드 주요 로직

**데이터 흐름**:
```
mounted() 
  └─> loadConventions()
      
onConventionChange()
  └─> loadAttributeDefinitions()
      └─> loadGuests()
      
onSaveClick()
  └─> submitBulkAssign()
      └─> POST /api/guest/bulk-assign-attributes
          └─> loadGuests() (refresh)
```

**주요 Computed Properties**:
- `filteredGuests`: 검색어 기반 필터링
- `allSelected`: 전체 선택 상태

---

## 🔐 보안 고려사항

### 1. 인증 및 권한
- JWT 토큰 기반 인증
- Admin Role 필수
- Controller에 `[Authorize(Roles = "Admin")]` 적용

### 2. 입력 검증
- 참석자 ID 존재 여부 확인
- ConventionId 검증
- SQL Injection 방지 (Entity Framework 사용)

### 3. 트랜잭션
- 데이터 일관성 보장
- 오류 시 자동 롤백

---

## 🚀 향후 개선 사항

### 1. 엑셀 업로드 기능
- ClosedXML 라이브러리 활용
- 템플릿 다운로드
- 대량 데이터 처리

### 2. 개별 편집 기능
- 일괄 설정 후 개별 수정
- 인라인 편집

### 3. 이력 관리
```sql
CREATE TABLE GuestAttributeHistory (
    Id INT PRIMARY KEY IDENTITY,
    GuestId INT,
    AttributeKey NVARCHAR(100),
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    ChangedBy NVARCHAR(100),
    ChangedAt DATETIME2
);
```

### 4. 통계 대시보드
- 속성별 인원 분포
- 미배정 참석자 수
- 실시간 현황

---

## 📞 문의

구현 관련 문의사항이 있으시면 개발팀으로 연락주세요.

---

**작성일**: 2025-10-14  
**버전**: 1.0.0  
**작성자**: Development Team

---

## 🎓 학습 포인트 정리

### C# / ASP.NET Core
1. **DTO 패턴**: 계층 간 데이터 전송
2. **트랜잭션 관리**: `BeginTransaction()`, `Commit()`, `Rollback()`
3. **LINQ 활용**: 효율적인 데이터 쿼리
4. **비동기 프로그래밍**: `async/await`

### Vue.js
1. **Computed Properties**: 파생 데이터 계산
2. **반응형 데이터**: 자동 UI 업데이트
3. **이벤트 처리**: `@click`, `@change`
4. **조건부 렌더링**: `v-if`, `v-for`, `v-show`

### 프론트엔드 UX
1. **로딩 상태**: 스피너 표시
2. **실시간 피드백**: 토스트 알림
3. **미리보기**: 저장 전 확인
4. **반응형 디자인**: 모바일 대응

### 데이터베이스
1. **외래 키 관계**: FK 설정
2. **인덱스 최적화**: 검색 성능 향상
3. **JSON 타입**: 유연한 데이터 저장

---

## 🔥 핵심 코드 스니펫

### 백엔드: 트랜잭션 처리
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try {
    // 기존 속성 삭제
    var attributesToRemove = existingAttributes
        .Where(ea => newAttributeKeys.Contains(ea.AttributeKey))
        .ToList();
    _context.GuestAttributes.RemoveRange(attributesToRemove);
    
    // 새 속성 추가
    await _context.GuestAttributes.AddRangeAsync(newAttributes);
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
} catch {
    await transaction.RollbackAsync();
    throw;
}
```

### 프론트엔드: 검색 필터
```javascript
computed: {
  filteredGuests() {
    if (!this.searchText) return this.guests;
    const search = this.searchText.toLowerCase();
    return this.guests.filter(g => 
      g.guestName.toLowerCase().includes(search) ||
      (g.corpName && g.corpName.toLowerCase().includes(search)) ||
      (g.corpPart && g.corpPart.toLowerCase().includes(search))
    );
  }
}
```

### 프론트엔드: 일괄 할당
```javascript
async submitBulkAssign() {
  const payload = {
    conventionId: this.selectedConventionId,
    guestMappings: this.selectedGuests.map(guestId => ({
      guestId,
      attributes: filteredAttributes
    }))
  };
  
  const response = await axios.post(
    '/api/guest/bulk-assign-attributes', 
    payload
  );
  
  if (response.data.success) {
    this.showToast(response.data.message, 'success');
    await this.loadGuests(); // 새로고침
  }
}
```

---

**문서 완료** ✅
