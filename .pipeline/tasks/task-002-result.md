# Task 002 Result: ScheduleTimeline + ScheduleDetailModal 컴포넌트 추출

## 상태
DONE

## 생성/수정 파일
- **생성**: `ClientApp/src/components/convention/ScheduleTimeline.vue` (368줄)
- **생성**: `ClientApp/src/components/convention/ScheduleDetailModal.vue` (213줄)
- **수정**: `ClientApp/src/views/MySchedule.vue` (134줄 — 래퍼로 교체)

## 완료 기준 체크
1. `npm run build` 성공 — ✅
2. `npm run lint` 경고/에러 없음 — ✅ (기존 코드베이스와 동일한 Node.js 모듈 타입 경고만 있음)
3. ScheduleTimeline.vue가 독립적으로 데이터를 로드하고 타임라인을 렌더링 — ✅
4. ScheduleDetailModal.vue가 schedule prop으로 상세 정보를 표시 — ✅
5. MySchedule.vue가 두 컴포넌트를 조합하여 기존과 동일하게 작동 — ✅
6. 캘린더 뷰, 옵션투어, 참석자 모달, 이미지 갤러리 등 기존 기능 모두 유지 — ✅
7. 각 컴포넌트 파일 크기 400줄 이하 — ✅ (ScheduleTimeline: 368줄, ScheduleDetailModal: 213줄)

## 테스트 결과
- `npm run build`: 성공 (built in ~20s)
- `npm run lint`: 에러 없음

## 판단 기록
- `ScheduleTimeline.vue`에서 `useConventionStore`를 제거하고 `conventionId`를 prop으로 받도록 설계 — 태스크 요구사항 준수
- 캘린더 뷰 토글(`showCalendarView`)은 `defineExpose`로 외부에 노출하여 MySchedule.vue 헤더 버튼과 연동
- `ScheduleDetailModal.vue`에서 `go-to-seat` 이벤트를 emit하여 라우터 네비게이션을 부모(MySchedule.vue)에 위임 — SRP 준수
- `dimPastSchedules` prop 지원: `isPastSchedule()` 함수로 지난 일정 opacity 처리
- `attributes` prop 지원: ScheduleDetailModal 하단에 "내 배정 정보" 섹션 추가 (Task 003에서 AssignmentBadges 컴포넌트 연동 예정)
