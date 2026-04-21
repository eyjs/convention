# Task 001 Result: 백엔드 - GuestAttribute 실제 데이터 API 반환

## 상태
PASS

## 수정 파일
- `Services/UserProfile/UserProfileService.cs` (594-601줄)

## 변경 내용
`GetMyConventionInfoAsync` 메서드의 `attributes` 변수를 빈 배열에서 실제 GuestAttribute 데이터로 변경.

```csharp
// 전
// 게스트 속성은 관리자 전용 메타데이터 — 일반 사용자 응답에서 제외
var attributes = new List<object>();

// 후
// 게스트 속성 — 사용자 배정 정보 표시용 (더보기 화면 + 타임라인 뱃지)
var attributes = user.GuestAttributes
    .Select(ga => new
    {
        key = ga.AttributeKey,
        value = ga.AttributeValue
    })
    .ToList();
```

`user`는 이미 `.Include(u => u.GuestAttributes)`로 로드되어 있으므로 추가 쿼리 없음.

## 완료 기준 체크
- [x] `dotnet build --no-restore` 성공 (오류 0개, 경고 53개는 기존)
- [x] `attributes` 필드에 `{ key, value }` 형태의 실제 GuestAttribute 데이터 반환
- [x] GuestAttribute가 없는 사용자는 빈 배열 반환 (LINQ Select + ToList가 빈 컬렉션 처리)

## 테스트 결과
- 빌드: 성공 (00:00:11)
- 수동 검증: 수동 검증 필요 (GuestAttribute 있는/없는 사용자 양쪽 확인)
