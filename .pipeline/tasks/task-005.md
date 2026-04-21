# Task 005: ConventionHome 재작성

## 목표
ConventionHome.vue를 컴팩트 헤더 + 타임라인 일정 조합으로 재작성한다. 기존 대시보드 요소(내 정보 카드, 공지 미리보기, 일정 미리보기 등)를 제거하고 타임라인을 메인 콘텐츠로 배치한다.

## 의존성
- 선행 태스크: Task 002 (ScheduleTimeline, ScheduleDetailModal), Task 003 (ConventionHeader, AssignmentBadges)
- 외부 의존성: DynamicActionRenderer, conventionStore, authStore

## 구현 상세

### 수정할 파일
1. **`ClientApp/src/views/ConventionHome.vue`** — 대폭 축소 재작성 (~300줄)

### 구현 지침

#### 전체 구조
```html
<template>
  <!-- 로딩 스피너 -->
  <!-- ConventionHeader (컴팩트 다크 헤더) -->
  <!-- GLOBAL_ROOT_POPUP 동적 액션 -->
  <!-- 여행 가이드 링크 (유지) -->
  <!-- HOME_SUB_HEADER 동적 액션 -->
  <!-- HOME_CONTENT_TOP 동적 액션 -->
  <!-- ScheduleTimeline (메인 콘텐츠) -->
  <!-- ScheduleDetailModal -->
</template>
```

#### 데이터 로딩 (onMounted)
기존 6개 API 호출에서 필요한 것만 유지:
```javascript
await Promise.all([
  loadDynamicActions(),    // HOME_SUB_HEADER + HOME_CONTENT_TOP + GLOBAL_ROOT_POPUP
  loadMyInfo(),            // attributes 로딩 (타임라인 뱃지용)
  checkTravelGuide(),      // 여행 가이드 존재 여부
])
```

**제거할 데이터 로딩**:
- `loadTodaySchedules()` — 일정 미리보기 3개 제거 (ScheduleTimeline이 자체 로드)
- `loadRecentNotices()` — 공지 미리보기 제거 (게시판 탭으로 이동)
- `loadChecklist()` — 체크리스트 제거 (더보기로 이동)

#### ConventionHeader 사용
```html
<ConventionHeader
  :convention="convention"
  :d-day="dDay"
  @menu-click="handleMenuClick"
/>
```

#### ScheduleTimeline 사용
```html
<ScheduleTimeline
  :brand-color="brandColor"
  :is-admin="isAdmin"
  :convention-id="conventionId"
  :attributes="myAttributes"
  :highlight-current="true"
  :dim-past-schedules="true"
  @schedule-click="onScheduleClick"
/>
```

#### ScheduleDetailModal 사용
```html
<ScheduleDetailModal
  :schedule="selectedSchedule"
  :brand-color="brandColor"
  :is-admin="isAdmin"
  :attributes="getScheduleAttributes(selectedSchedule)"
  @close="selectedSchedule = null"
/>
```

#### 동적 액션 로딩
기존 `loadDynamicActions`에서 SCHEDULE_CONTENT_TOP도 추가 로딩:
```javascript
// HOME_SUB_HEADER + HOME_CONTENT_TOP + GLOBAL_ROOT_POPUP + SCHEDULE_CONTENT_TOP
const targetLocations = 'HOME_SUB_HEADER,HOME_CONTENT_TOP,GLOBAL_ROOT_POPUP,SCHEDULE_CONTENT_TOP'
```
- SCHEDULE_CONTENT_TOP은 ScheduleTimeline 내부에서 자체 로딩하므로 ConventionHome에서는 별도 처리 불필요할 수 있음 (Task 002 구현에 따라 결정)

#### 배정 정보 -> 타임라인 뱃지 연결
```javascript
const myAttributes = computed(() => {
  if (!myInfo.value?.attributes) return []
  return myInfo.value.attributes  // [{ key, value }]
})
```

#### 제거할 요소
- 배너 이미지 헤더 (h-48, conventionImg 배경) -> ConventionHeader로 교체
- 내 정보 카드 (myInfo 전체 표시, 여권, 동반자, 옵션투어) -> 더보기로 이동
- 여권 상세 모달 (BaseModal) -> 더보기 또는 내 프로필에서 접근
- 체크리스트 (ChecklistProgress) -> 더보기로 이동
- 설문조사 미완료 섹션 -> 더보기로 이동
- 공지사항 미리보기 -> 제거 (게시판 탭에서 접근)
- 나의 일정 미리보기 3개 -> 제거 (타임라인으로 대체)
- 관련 import 제거: DeadlineCountdown, ChecklistProgress, BaseModal

#### 유지할 요소
- GLOBAL_ROOT_POPUP 동적 액션
- HOME_SUB_HEADER 동적 액션
- HOME_CONTENT_TOP 동적 액션
- 여행 가이드 링크 (hasTravelGuide)
- brandColor computed
- dDay computed
- convention computed (conventionStore)

### 레이아웃 구조
```
[ConventionHeader - 컴팩트, brandColor 배경]
[여행 가이드 링크 (있을 때만)]
[HOME_SUB_HEADER 동적 액션 (있을 때만)]
[HOME_CONTENT_TOP 동적 액션 (있을 때만)]
[ScheduleTimeline - 전체 높이 사용, 날짜 필터 + 타임라인]
[ScheduleDetailModal - 오버레이]
```

## 기술 제약
- JavaScript (TypeScript 아님)
- Composition API `<script setup>`
- Tailwind CSS
- ConventionHome.vue 최종 크기 300줄 이하 목표 (최대 400줄)
- 기존 ConventionHome의 라우트명 `ConventionHome` 유지
- `showNav: true` 메타 유지

## 완료 기준
1. `npm run build` 성공
2. `npm run lint` 경고/에러 없음
3. 행사 홈 진입 시 컴팩트 헤더 + 타임라인 일정이 즉시 표시
4. 기존 대시보드 요소(내 정보, 공지, 일정 미리보기)가 제거됨
5. Dynamic Action 3개 위치(GLOBAL_ROOT_POPUP, HOME_SUB_HEADER, HOME_CONTENT_TOP)가 정상 작동
6. 타임라인에서 일정 클릭 시 상세 모달이 열림
7. 배정 정보 뱃지가 일정 카드에 표시됨 (GuestAttribute 있는 경우)
8. 여행 가이드 링크가 유지됨
9. 파일 크기 400줄 이하

## 테스트 요구사항
- 빌드 검증: `cd ClientApp && npm run build && npm run lint`
- 수동 검증:
  - 행사 홈 진입 시 로딩 -> 헤더 + 타임라인 표시
  - 일정 클릭 -> 상세 모달
  - 동적 액션 렌더링
  - 다양한 brandColor에서 헤더 가독성
