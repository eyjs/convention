# Task 002: 컴포넌트 추출 - ScheduleTimeline + ScheduleDetailModal

## 목표
`MySchedule.vue`(1113줄)에서 타임라인 UI와 일정 상세 모달을 독립 컴포넌트로 추출하여 ConventionHome에서 재사용 가능하게 한다.

## 의존성
- 선행 태스크: 없음
- 외부 의존성: dayjs, SlideUpModal, QuillViewer, ParticipantList, DynamicActionRenderer

## 구현 상세

### 생성할 파일
1. **`ClientApp/src/components/convention/ScheduleTimeline.vue`** (~400줄)
2. **`ClientApp/src/components/convention/ScheduleDetailModal.vue`** (~250줄)

### 수정할 파일
3. **`ClientApp/src/views/MySchedule.vue`** — 추출 후 컴포넌트 임포트로 교체

### ScheduleTimeline.vue 구현 지침

#### Props
```javascript
const props = defineProps({
  brandColor: { type: String, default: '#10b981' },
  isAdmin: { type: Boolean, default: false },
  conventionId: { type: [Number, String], required: true },
  // 배정 정보 (GuestAttribute key-value 배열) - 타임라인 뱃지 표시용
  attributes: { type: Array, default: () => [] },
  // 현재 진행중 일정 하이라이트 여부
  highlightCurrent: { type: Boolean, default: true },
  // 지난 일정 흐림 처리 여부
  dimPastSchedules: { type: Boolean, default: false },
})
```

#### Emits
```javascript
const emit = defineEmits(['schedule-click'])
```

#### 포함할 기능 (MySchedule.vue에서 이동)
- 일정 + 옵션투어 데이터 로딩 (`loadSchedules`, `loadOptionTours`)
- 날짜 탭 가로 스크롤 (dateScrollContainer, showLeftScroll, showRightScroll)
- 날짜별 그룹화 (groupedSchedules computed)
- 현재 진행 중 일정 계산 (currentSchedule computed, dayjs 사용)
- 현재 일정 자동 스크롤 (watch + scrollIntoView)
- 캘린더 뷰 토글 (showCalendarView, calendarDays, changeMonth)
- 타임라인 카드 UI (bullet + 카드 + 이미지 썸네일)
- 전체 이미지 보기 오버레이 (fullImageUrl)
- SCHEDULE_CONTENT_TOP 동적 액션 로딩 및 렌더링
- 일정 데이터 합산 (mergedSchedules: 일반일정 + 옵션투어)
- **신규**: `dimPastSchedules` prop이 true일 때 지난 일정 opacity 낮춤 (목업의 `.done { opacity: .32 }`)
- **신규**: `attributes` prop으로 받은 배정 정보를 일정 카드에 인라인 뱃지로 표시 (AssignmentBadges 컴포넌트 사용 - Task 003에서 생성)

#### 일정 카드 클릭 시
- `emit('schedule-click', schedule)` 발행 -> 부모가 ScheduleDetailModal 오픈

#### 내부 상태로 관리 (부모에 노출 안 함)
- selectedDate, dates, showCalendarView, calendarDays 등 날짜 필터 관련
- dateScrollContainer ref, 스크롤 인디케이터
- fullImageUrl (이미지 전체보기)
- allSchedules, allOptionTours (데이터)
- allActions (SCHEDULE_CONTENT_TOP 동적 액션)

### ScheduleDetailModal.vue 구현 지침

#### Props
```javascript
const props = defineProps({
  schedule: { type: Object, default: null }, // null이면 모달 닫힘
  brandColor: { type: String, default: '#10b981' },
  isAdmin: { type: Boolean, default: false },
  // 배정 정보 (해당 일정에 연결된 속성)
  attributes: { type: Array, default: () => [] },
})
```

#### Emits
```javascript
const emit = defineEmits(['close'])
```

#### 포함할 기능 (MySchedule.vue에서 이동)
- SlideUpModal 래핑
- 일정 상세 표시 (날짜, 시간, 장소, 설명, 이미지)
- QuillViewer로 HTML 설명 렌더링
- 이미지 갤러리 + 전체 이미지 보기
- 내 자리 보기 버튼 (seatingLayoutId)
- 참석자 보기 (관리자만) - ParticipantList + SlideUpModal 중첩
- **신규**: 하단에 "내 배정 정보" 섹션 (목업의 m-my 영역) - attributes prop 기반

### MySchedule.vue 수정 지침
- 추출 완료 후 MySchedule.vue를 ScheduleTimeline + ScheduleDetailModal 조합으로 교체
- MySchedule.vue는 라우트가 유지될 수 있으므로 (redirect 전까지) 최소한의 래퍼로 남겨둠
- 실제 라우트 제거는 Task 006에서 수행

## 기술 제약
- JavaScript (TypeScript 아님)
- Composition API `<script setup>`
- Tailwind CSS 스타일링
- dayjs 의존성 유지
- 기존 API 엔드포인트 동일 사용: `GET /user-schedules/{userId}/{conventionId}`, `GET /user-schedules/{userId}/{conventionId}/option-tours`

## 완료 기준
1. `npm run build` 성공
2. `npm run lint` 경고/에러 없음
3. ScheduleTimeline.vue가 독립적으로 데이터를 로드하고 타임라인을 렌더링
4. ScheduleDetailModal.vue가 schedule prop으로 상세 정보를 표시
5. MySchedule.vue가 두 컴포넌트를 조합하여 기존과 동일하게 작동
6. 캘린더 뷰, 옵션투어, 참석자 모달, 이미지 갤러리 등 기존 기능 모두 유지
7. 각 컴포넌트 파일 크기 400줄 이하

## 테스트 요구사항
- 빌드 검증: `cd ClientApp && npm run build && npm run lint`
- 수동 검증: 기존 일정 페이지 기능 동일 작동 확인
