# 일정 업로드 Upsert + GuestAttribute ConventionId 격리

## 생성일시
2026-04-27 00:00

## 목적
- 두 가지 독립적 이슈를 한 번에 해결
- **이슈 1**: 일정 업로드에 참석자 업로드와 동일한 Upsert/전체교체 모드 추가
- **이슈 2**: GuestAttribute에 ConventionId 누락으로 행사 간 배정 정보가 교차 노출되는 버그 수정

## 스코프

### 포함 (이번에 만드는 것)

#### 이슈 1 — 일정 업로드 Upsert 모드
- [ ] 일정 업로드 시 `replaceAll` 파라미터 추가 (참석자 업로드와 동일 패턴)
- [ ] **기본 모드 (Upsert)**: 동일 CourseName의 기존 ScheduleTemplate이 있으면 ScheduleItem 업데이트/추가, 없으면 신규 생성
- [ ] **전체 교체 모드 (replaceAll=true)**: 해당 행사의 모든 ScheduleTemplate + ScheduleItem 삭제 후 재생성
- [ ] 프론트엔드 BulkUpload.vue의 "전체 교체 모드" 체크박스를 일정 업로드에도 적용
- [ ] 결과 DTO에 TemplatesUpdated, ItemsUpdated 카운트 추가

#### 이슈 2 — GuestAttribute ConventionId 격리
- [ ] GuestAttribute 엔티티에 `ConventionId` 컬럼 추가 (FK, nullable → 마이그레이션 후 기존 데이터 보정)
- [ ] DB 마이그레이션: ConventionId 추가 + 기존 데이터 UserConvention 기반 보정
- [ ] GuestAttributeRepository 모든 메서드에 conventionId 파라미터 추가
- [ ] AttributeCategoryService.GetGroupedAttributesAsync — ConventionId 필터 추가
- [ ] UserProfileService.GetMyConventionInfoAsync — ConventionId 필터 추가
- [ ] UserUploadService/AttributeUploadService — 속성 생성 시 ConventionId 저장
- [ ] 유니크 제약 조건: (UserId, AttributeKey) → (ConventionId, UserId, AttributeKey)

### 제외 (이번에 만들지 않는 것)
- 참석자 업로드(UserUploadService) 로직 변경
- 일정 업로드 Preview 단계 변경 (기존 유지)
- 프론트엔드 "내 배정 정보" 화면 UI 변경 (데이터만 올바르게 필터링)

## 기술스택
- 백엔드: .NET 8 / ASP.NET Core (C#), EF Core, SQL Server
- 프론트엔드: Vue 3 Composition API (JavaScript), Tailwind CSS
- 기존 코드 재사용:
  - `ScheduleUploadService.cs` — ConfirmMultiSheetAsync에 upsert 로직 추가
  - `UploadController.cs` — replaceAll 파라미터 추가
  - `UserUploadService.cs` — replaceAll 패턴 참조
  - `GuestAttributeRepository.cs` — conventionId 파라미터 추가
  - `BulkUpload.vue` — 전체교체 체크박스 패턴 참조

## 핵심 기능

### P0 (필수)

#### 이슈 1 — 일정 업로드 Upsert

**백엔드 변경:**

1. `UploadController.cs` — confirm-multi 엔드포인트에 `replaceAll` 쿼리 파라미터 추가
   ```
   POST /upload/schedule-templates/confirm-multi?replaceAll=false (기본)
   POST /upload/schedule-templates/confirm-multi?replaceAll=true  (전체교체)
   ```

2. `ScheduleUploadService.ConfirmMultiSheetAsync` — upsert 로직 추가
   - **replaceAll=true 일 때:**
     1. 해당 conventionId의 모든 GuestScheduleTemplate 삭제
     2. 해당 conventionId의 모든 ScheduleItem 삭제 
     3. 해당 conventionId의 모든 ScheduleTemplate 삭제
     4. 새로 생성 (기존 로직)
   - **replaceAll=false (Upsert) 일 때:**
     1. 시트별 CourseName으로 기존 ScheduleTemplate 조회
     2. 기존 템플릿 있으면: 기존 ScheduleItem 모두 삭제 후 새 항목으로 교체
     3. 기존 템플릿 없으면: 신규 생성 (기존 로직)

3. `ScheduleTemplateUploadResult` DTO 확장
   ```csharp
   public int TemplatesUpdated { get; set; }  // 추가
   public int ItemsUpdated { get; set; }      // 추가 (= 기존 템플릿의 새 아이템 수)
   public int TemplatesDeleted { get; set; }  // 추가 (replaceAll 시)
   public int ItemsDeleted { get; set; }      // 추가 (replaceAll 시)
   ```

4. `IScheduleTemplateUploadService` 인터페이스 — replaceAll 파라미터 추가

**프론트엔드 변경:**

5. `BulkUpload.vue` — 일정 업로드 모드에서도 "전체 교체 모드" 체크박스 표시
   - 기존 참석자 업로드용 체크박스 로직을 일정 업로드에도 적용
   - confirm-multi API 호출 시 `?replaceAll=true/false` 전달
   - 결과 표시에 Updated 카운트 포함

#### 이슈 2 — GuestAttribute ConventionId 격리

**엔티티 + 마이그레이션:**

1. `Entities/GuestAttribute.cs` — ConventionId 프로퍼티 추가
   ```csharp
   public int? ConventionId { get; set; }  // nullable (기존 데이터 호환)
   
   [ForeignKey("ConventionId")]
   public Convention? Convention { get; set; }
   ```

2. DB 마이그레이션 생성
   - ConventionId nullable 컬럼 추가
   - 기존 데이터: UserConvention 조인으로 ConventionId 보정 (SQL UPDATE)
   - 인덱스: IX_GuestAttributes_ConventionId_UserId_AttributeKey (Unique)

**Repository 변경:**

3. `IGuestAttributeRepository` — 모든 메서드에 conventionId 파라미터 추가
   ```csharp
   Task<IEnumerable<GuestAttribute>> GetAttributesByUserIdAsync(int userId, int conventionId, ...)
   Task<GuestAttribute?> GetAttributeByKeyAsync(int userId, int conventionId, string attributeKey, ...)
   Task UpsertAttributeAsync(int userId, int conventionId, string attributeKey, string attributeValue, ...)
   ```

4. `GuestAttributeRepository` — WHERE 조건에 ConventionId 추가

**서비스 변경:**

5. `AttributeCategoryService.GetGroupedAttributesAsync` (line 223)
   - `ga.UserId == userId` → `ga.UserId == userId && ga.ConventionId == conventionId`

6. `UserProfileService.GetMyConventionInfoAsync` (line 547, 599)
   - GuestAttributes Include → ConventionId 필터 추가

7. `UserUploadService` — 속성 생성 시 ConventionId 전달

8. `AttributeUploadService` — 속성 생성 시 ConventionId 전달

9. 기타 GuestAttribute를 사용하는 모든 서비스 — conventionId 전달

## 제약사항
- 프론트엔드는 JavaScript (TypeScript 아님), `<script setup>` Composition API 필수
- GuestAttribute.ConventionId는 nullable로 시작 (기존 데이터 호환)
- 마이그레이션에서 기존 데이터 보정 SQL 포함 필수
- 일정 Upsert 매칭 키: ConventionId + CourseName (템플릿 레벨)
- Upsert 모드에서 기존 템플릿의 아이템은 전체 교체 (아이템 레벨 매칭은 복잡도 대비 이점 없음)
- Tailwind CSS 사용, 하드코딩 색상값 금지

## 성공 기준
1. 일정 업로드 기본 모드: 동일 CourseName 재업로드 시 기존 템플릿의 아이템이 업데이트됨
2. 일정 업로드 전체교체 모드: 체크박스 선택 시 기존 일정 전체 삭제 후 재생성
3. "내 배정 정보": 행사 A의 속성이 행사 B에 노출되지 않음
4. 기존 데이터: 마이그레이션으로 ConventionId 보정 완료
5. `dotnet build --no-restore` 통과
6. `npm run build && npm run lint` 통과

## 특이사항

### 이슈 1 — Upsert 매칭 전략
- **템플릿 레벨**: CourseName + ConventionId로 기존 템플릿 조회
- **아이템 레벨**: 개별 매칭 대신 "기존 아이템 전체 삭제 → 새 아이템 삽입" (코스 단위 교체)
- 이유: 날짜+시간+제목 매칭은 사용자가 시간 변경 시 매칭 실패, 복잡도 대비 이점 없음
- 참석자 업로드의 replaceAll 패턴(UserUploadService lines 85-118)을 그대로 차용

### 이슈 2 — 기존 데이터 보정 SQL
```sql
-- 마이그레이션 내 SQL로 기존 GuestAttribute에 ConventionId 채우기
UPDATE ga
SET ga.ConventionId = uc.ConventionId
FROM GuestAttributes ga
INNER JOIN UserConventions uc ON ga.UserId = uc.UserId
WHERE ga.ConventionId IS NULL
```
- 한 사용자가 여러 행사에 참여한 경우: 가장 최근 행사의 ConventionId로 배정 (또는 중복 생성)
- 정확한 보정은 운영 데이터 확인 후 결정

### 영향 범위 (GuestAttribute 사용처)
| 서비스 | 메서드 | 변경 필요 |
|--------|--------|-----------|
| AttributeCategoryService | GetGroupedAttributesAsync | ConventionId 필터 추가 |
| UserProfileService | GetMyConventionInfoAsync | ConventionId 필터 추가 |
| UserProfileService | BulkAssignAttributesAsync | ConventionId 저장 |
| UserUploadService | UploadUsersAsync (인라인 속성) | ConventionId 저장 |
| AttributeUploadService | UploadAttributesAsync | ConventionId 저장 |
| GuestAttributeRepository | 모든 메서드 | conventionId 파라미터 추가 |
| TemplateVariableService | ReplaceVariables (읽기) | ConventionId 필터 추가 |
| SmsGroupService | 엑셀 변수 발송 (읽기) | ConventionId 필터 추가 |
