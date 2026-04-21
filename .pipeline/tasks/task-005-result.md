# Task 005 Result: ConventionHome 재작성

## 상태
DONE

## 수정 파일
- `ClientApp/src/views/ConventionHome.vue` — 1,162줄 → 214줄으로 재작성

## 완료 기준 체크
- [x] `npm run build` 성공
- [x] `npm run lint` 경고/에러 없음 (기존 module type 경고만 존재, 무관)
- [x] ConventionHeader + ScheduleTimeline + ScheduleDetailModal 조합
- [x] GLOBAL_ROOT_POPUP, HOME_SUB_HEADER, HOME_CONTENT_TOP 동적 액션 유지
- [x] 파일 크기 214줄 (목표 400줄 이하)
- [x] 라우트명 ConventionHome 유지 (router/index.js 미수정)
- [x] showNav: true 메타 유지 (router/index.js 미수정)
- [x] 타임라인 일정 클릭 시 ScheduleDetailModal 열림
- [x] myAttributes(배정 정보)를 타임라인/모달에 prop으로 전달
- [x] 여행 가이드 링크 유지

## 제거된 요소
- 배너 이미지 헤더 (h-48) → ConventionHeader로 교체
- 내 정보 카드 (탭, 여권, 동반자, 옵션투어)
- 여권 상세 모달 (BaseModal)
- 체크리스트 진행상황 (ChecklistProgress)
- 설문조사 미완료 섹션
- 공지사항 미리보기
- 나의 일정 미리보기 3개

## 제거된 import
- DeadlineCountdown, ChecklistProgress, BaseModal, MainHeader
- useNoticeNavigation

## 제거된 데이터 로딩
- loadTodaySchedules()
- loadRecentNotices()
- loadChecklist()

## 유지된 요소
- convention 데이터 (conventionStore)
- brandColor computed
- dDay computed
- Dynamic Action 로딩 (HOME_SUB_HEADER, HOME_CONTENT_TOP, GLOBAL_ROOT_POPUP)
- 여행 가이드 링크 (hasTravelGuide)
- SidebarMenu 연동 (@menu-click → isSidebarOpen = true)

## 테스트 결과
- `npm run build`: 성공 (18.62s)
- `npm run lint`: 성공 (--fix 적용, 에러 없음)
