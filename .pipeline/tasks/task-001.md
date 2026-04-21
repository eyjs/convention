# Task 001: 백엔드 - GuestAttribute 실제 데이터 API 반환

## 목표
`UserProfileService.GetMyConventionInfoAsync`에서 `attributes`를 빈 배열 대신 실제 GuestAttribute 키-값 데이터로 반환한다.

## 의존성
- 선행 태스크: 없음
- 외부 의존성: 없음

## 구현 상세

### 수정할 파일
1. **`Services/UserProfile/UserProfileService.cs`** (594-595줄 부근)

### 구현 지침

현재 코드 (594-595줄):
```csharp
// 게스트 속성은 관리자 전용 메타데이터 — 일반 사용자 응답에서 제외
var attributes = new List<object>();
```

변경할 코드:
```csharp
// 게스트 속성 — 사용자 배정 정보 표시용
var attributes = user.GuestAttributes
    .Select(ga => new
    {
        key = ga.AttributeKey,
        value = ga.AttributeValue
    })
    .ToList();
```

**주의**: `user`는 이미 `.Include(u => u.GuestAttributes)`로 로드되어 있으므로 (543줄) 추가 쿼리 불필요.

주석도 함께 수정:
- "관리자 전용 메타데이터 — 일반 사용자 응답에서 제외" -> "사용자 배정 정보 표시용 (더보기 화면 + 타임라인 뱃지)"

## 기술 제약
- 기존 API 응답 구조의 `attributes` 필드명은 동일하게 유지 (프론트엔드 호환)
- 배열 요소의 형태: `{ key: string, value: string }` (기존 빈 배열이었으므로 하위호환 OK)

## 완료 기준
1. `dotnet build --no-restore` 성공
2. `GET /api/users/my-convention-info/{conventionId}` 응답의 `attributes` 필드에 실제 GuestAttribute 데이터가 `[{ key: "룸번호", value: "312" }, ...]` 형태로 포함됨
3. GuestAttribute가 없는 사용자는 빈 배열 `[]` 반환 (에러 없음)

## 테스트 요구사항
- 빌드 검증: `dotnet build --no-restore`
- 수동 검증: GuestAttribute가 있는 사용자와 없는 사용자 양쪽 확인
