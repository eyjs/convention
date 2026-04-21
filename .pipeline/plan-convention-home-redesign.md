# 실행 계획: 행사 홈 화면 구조 개편

## 요구사항 요약

행사 홈(ConventionHome) 진입 시 바로 타임라인 일정이 보이도록 구조 개편.
- 행사 홈 = 컴팩트 헤더 + 타임라인 일정 (기존 대시보드 제거)
- 하단 탭 4개(홈/일정/게시판/더보기) -> 3개(홈/게시판/더보기)
- 더보기 = 내 정보 + 배정 정보(GuestAttribute) + 메뉴 섹션
- 배정 정보 API에서 실제 GuestAttribute 데이터 반환

## 기술적 분석

### 영향 범위
| 영역 | 파일 | 변경 규모 |
|------|------|-----------|
| 백엔드 API | `UserProfileService.cs` | 소 (attributes 빈배열 -> 실제 데이터) |
| 새 컴포넌트 | `components/convention/` 4개 | 중 (신규 생성) |
| 뷰 재작성 | `ConventionHome.vue` | 대 (1162줄 -> 컴포넌트 조합 300줄 이하) |
| 뷰 재작성 | `MoreFeaturesView.vue` | 대 (213줄 -> 전면 재작성) |
| 뷰 추출 | `MySchedule.vue` | 대 (타임라인/모달을 컴포넌트로 추출) |
| 레이아웃 | `ConventionLayout.vue` | 소 (navItems 4->3) |
| 라우터 | `router/index.js` | 소 (schedule redirect) |
| 스키마 | `targetLocations.js` | 소 (설명 텍스트 수정) |

### 의존성 그래프
```
Task 001 (백엔드 API) ──────────────────────────────┐
Task 002 (컴포넌트 추출: ScheduleTimeline + Modal) ──┤
Task 003 (컴포넌트 생성: ConventionHeader + Badges) ─┤
                                                      ├── Task 005 (ConventionHome 재작성)
Task 004 (MoreFeaturesView 재작성) ──────────────────┘  │
Task 006 (라우터 + 레이아웃 + targetLocations) ──────────┘
```

### 리스크
1. **MySchedule.vue에서 컴포넌트 추출 시 기존 기능 손실** - 캘린더뷰, 옵션투어, 참석자 모달, 이미지 갤러리 등 복잡한 기능이 얽혀 있음
2. **Dynamic Action 시스템 깨짐** - GLOBAL_ROOT_POPUP, HOME_SUB_HEADER, HOME_CONTENT_TOP, SCHEDULE_CONTENT_TOP, MORE_FEATURES_GRID 등 5개 위치가 영향받음
3. **ConventionHome 크기 관리** - 타임라인 통합 시 컴포넌트 분리가 미흡하면 1800줄+ 위험

### 선행 조건
- 기존 `dotnet build` 및 `npm run build` 통과 확인
- 목업 HTML 디자인 참조

## 아키텍처 결정

### 결정 1: 컴포넌트 분리 전략
**결정**: MySchedule.vue에서 타임라인 UI와 상세 모달을 독립 컴포넌트로 추출
**근거**: ConventionHome이 1800줄 넘지 않도록. ScheduleTimeline은 날짜 필터 + 타임라인 + 캘린더뷰 + 옵션투어 통합 로직을 포함. ScheduleDetailModal은 상세 모달 + 참석자 모달 + 이미지 뷰어 포함.

### 결정 2: 배정 뱃지 구현 방식
**결정**: 별도 AssignmentBadges.vue 컴포넌트로 분리, GuestAttribute 키-값을 색상 매핑하여 인라인 뱃지 렌더링
**근거**: 목업에서 룸번호/룸메이트/골프조 등 다양한 속성을 일정 카드와 상세 모달 양쪽에서 사용하므로 재사용성 필요

### 결정 3: SCHEDULE_CONTENT_TOP 동적 액션 이동
**결정**: ConventionHome에서 HOME_CONTENT_TOP과 SCHEDULE_CONTENT_TOP을 모두 로드하여 타임라인 위에 렌더링
**근거**: 일정 탭이 제거되므로 기존 SCHEDULE_CONTENT_TOP 액션이 표시될 곳이 필요

### 결정 4: MoreFeaturesView 구조
**결정**: 3개 섹션(내 정보 + 배정 정보 + 메뉴 섹션)으로 전면 재작성. 메뉴 섹션은 리스트 행 형태.
**근거**: 목업 디자인 따름. 기존 2x2 그리드 -> 리스트 행으로 변경.

## 구현 전략

Phase 순서: 백엔드 먼저 -> 컴포넌트 추출/생성 (병렬 가능) -> 뷰 조합 -> 라우터/레이아웃 -> 빌드 검증

## 단계별 실행 계획

### Phase 1: 백엔드 API 수정 (Task 001)
- **목표**: GuestAttribute 실제 데이터를 API 응답에 포함
- **작업**: `UserProfileService.GetMyConventionInfoAsync`에서 `attributes` 빈 배열 -> 실제 데이터
- **완료 기준**: `dotnet build --no-restore` 통과, API 응답에 attributes 키-값 포함

### Phase 2: 컴포넌트 추출 및 생성 (Task 002, 003 병렬)
- **목표**: MySchedule.vue에서 재사용 가능한 컴포넌트 추출 + 신규 컴포넌트 생성
- **작업**:
  - Task 002: ScheduleTimeline.vue + ScheduleDetailModal.vue 추출
  - Task 003: ConventionHeader.vue + AssignmentBadges.vue 신규 생성
- **완료 기준**: 각 컴포넌트가 독립적으로 임포트 가능, `npm run build` 통과

### Phase 3: 뷰 재작성 (Task 004, 005)
- **목표**: MoreFeaturesView 전면 재작성 + ConventionHome 재작성
- **작업**:
  - Task 004: MoreFeaturesView.vue 전면 재작성 (내 정보 + 배정 + 메뉴)
  - Task 005: ConventionHome.vue 재작성 (컴팩트 헤더 + 타임라인 조합)
- **완료 기준**: `npm run build && npm run lint` 통과, 각 뷰 400줄 이하

### Phase 4: 라우터/레이아웃 통합 (Task 006)
- **목표**: 탭 변경 + 라우트 정리 + 빌드 검증
- **작업**: ConventionLayout 탭 수정, router schedule redirect, targetLocations 수정
- **완료 기준**: 전체 빌드 통과 (`dotnet build --no-restore` + `npm run build && npm run lint`)

## 예상 태스크 목록

| 태스크 | 이름 | 의존성 | 복잡도 | 파일 수 |
|--------|------|--------|--------|---------|
| 001 | 백엔드: GuestAttribute API 반환 | 없음 | 소 | 1 |
| 002 | 컴포넌트 추출: ScheduleTimeline + ScheduleDetailModal | 없음 | 대 | 3 (2 신규 + MySchedule 수정) |
| 003 | 컴포넌트 생성: ConventionHeader + AssignmentBadges | 없음 | 중 | 2 신규 |
| 004 | MoreFeaturesView 전면 재작성 | 001 | 중 | 1 |
| 005 | ConventionHome 재작성 | 002, 003 | 대 | 1 |
| 006 | 라우터 + 레이아웃 + targetLocations | 005 | 소 | 3 |
