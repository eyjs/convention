# Task 004 Result: MoreFeaturesView 전면 재작성

## 상태
완료 (DONE)

## 생성/수정 파일
- **수정**: `ClientApp/src/views/MoreFeaturesView.vue` — 전면 재작성 (~250줄)

## 완료 기준 체크
- [x] `npm run build` 성공 (18.54s)
- [x] `npm run lint` 경고/에러 없음 (module type warning은 기존 설정 문제로 무관)
- [x] 내 정보 + 배정 정보 + 메뉴 3개 섹션 렌더링
- [x] 배정 정보에 GuestAttribute 키-값이 2열 그리드로 표시됨
- [x] 속성이 없는 사용자는 배정 정보 섹션이 숨겨짐 (`v-if="hasAttributes"`)
- [x] 설문조사/체크리스트/동적 메뉴 클릭이 정상 작동
- [x] 동반자 토글이 정상 작동 (`isCompanionsOpen` 상태 토글)
- [x] 파일 크기 400줄 이하 (~250줄)

## 테스트 결과
- 빌드: ✓ `built in 18.54s`
- 린트: ✓ 에러 없음 (기존 module type 경고만 출력)

## 구현 내용

### 섹션 1: 내 정보
- 아이콘(보라) + 이름 + 역할
- 아이콘(초록) + 코스 (scheduleCourses 있을 때만)
- 아이콘(황금) + 그룹명 (groupName 있을 때만)
- 아이콘(초록) + 동반자 토글 (companions 있을 때만, ChevronDown 회전 애니메이션)

### 섹션 2: 내 배정 정보
- `v-if="hasAttributes"` — 속성 없으면 섹션 자체 숨김
- 2열 그리드, AssignmentBadges.vue와 동일한 COLOR_PALETTE 순환
- 값 없으면 "미배정" (회색) 표시

### 섹션 3: 메뉴
- `v-if="hasMenuItems"` — 메뉴가 하나도 없으면 섹션 숨김
- 설문조사: hasSurveys 시 노출, 미완료 수 뱃지
- 체크리스트: checklistMeta 시 노출, 완료/전체 뱃지
- Dynamic Action MENU: 기존 executeAction composable 재사용

### 제거된 것
- 2x2 그리드 레이아웃 → 리스트 행 형태로 전환
- CATEGORY_ICONS 맵
- MainHeader 컴포넌트 (ConventionLayout에서 관리)

## 판단 기록
- `checklist-status` API는 별도 엔드포인트가 없을 수 있어 기존 `actions/menu`의 CHECKLIST_CARD 파싱 우선 처리하고, API 응답이 있으면 fallback으로 사용
- task-004의 `더보기` 헤더를 ConventionLayout이 관리하므로 MainHeader 제거
