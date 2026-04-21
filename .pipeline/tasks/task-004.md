# Task 004: MoreFeaturesView 전면 재작성

## 목표
더보기 화면을 "내 정보 + 배정 정보 + 메뉴 섹션" 3단 구조로 전면 재작성한다.

## 의존성
- 선행 태스크: Task 001 (attributes API 반환)
- 외부 의존성: 기존 API 엔드포인트, DynamicActionRenderer, useAction composable

## 구현 상세

### 수정할 파일
1. **`ClientApp/src/views/MoreFeaturesView.vue`** — 전면 재작성 (~350줄)

### 구현 지침

#### 데이터 로딩
기존 MoreFeaturesView의 API 호출 유지 + myInfo API 추가:
```javascript
const [myInfoRes, actionsRes, statusesRes, surveysRes, checklistRes] = await Promise.all([
  apiClient.get(`/users/my-convention-info/${conventionId}`),
  apiClient.get(`/conventions/${conventionId}/actions/menu`),
  apiClient.get(`/conventions/${conventionId}/actions/statuses`),
  apiClient.get(`/surveys/convention/${conventionId}`).catch(() => ({ data: [] })),
  apiClient.get(`/conventions/${conventionId}/actions/checklist-status`).catch(() => ({ data: null })),
])
```

#### 섹션 1: 내 정보 (목업의 more-card)
- **디자인**: 흰색 카드, 리스트 행 형태
- **행 1**: 아이콘(보라) + 이름 + 역할(소속/직책)
- **행 2**: 아이콘(초록) + "코스" + 배정된 일정 코스명 (scheduleCourses)
- **행 3**: 아이콘(황금) + "그룹" + groupName
- **행 4**: 동반자 보기 (토글) - companions 배열이 있을 때만 표시
  - 펼치면 동반자 리스트 (아바타 + 이름 + 관계타입)
- 아이콘 스타일: `w-7 h-7 rounded-lg flex items-center justify-center` + 배경색

#### 섹션 2: 내 배정 정보 (목업의 assign-card)
- **디자인**: 흰색 카드, 2열 그리드
- **데이터**: `myInfo.attributes` 배열 (Task 001에서 반환)
- 각 항목: 라벨(작은 글씨, 색상) + 값(큰 글씨, 볼드)
- 값이 없으면 "미배정" 표시 (회색)
- 색상: AssignmentBadges와 동일한 COLOR_PALETTE 순환
- 속성이 하나도 없으면 섹션 자체를 숨김

#### 섹션 3: 메뉴 (리스트 행 형태)
- **설문조사**: 발행된 설문이 있을 때만 1개 메뉴로 노출
  - 아이콘 + "설문조사" + 미완료 수 뱃지
  - 클릭 -> SurveyList 라우트
- **체크리스트**: 체크리스트 항목이 있을 때만 1개 메뉴로 노출
  - 아이콘 + "체크리스트" + 완료/전체 뱃지
  - 클릭 -> Checklist 라우트
- **Dynamic Action MENU 타입**: 기존 동적 메뉴들 유지
  - 아이콘 + 제목 + 상태 뱃지(완료/미완료)
  - 클릭 -> executeAction(item)
- 리스트 행 디자인: `flex items-center gap-3 px-4 py-3 border-b` (목업의 more-row)

#### 섹션 제목 스타일
- `text-xs font-medium text-gray-400 mb-2 px-1` (목업의 more-sec-t)
- "내 정보", "내 배정 정보", "메뉴" 등

#### 헤더
- 기존 MainHeader 유지: `title="더보기"` (show-back은 ConventionLayout에서 관리하므로 false)
- 또는 헤더 없이 ConventionLayout 내부이므로 간단히 처리

### 기존 코드에서 유지할 것
- `useAction` composable의 `executeAction` 함수
- Dynamic Action 메뉴 로딩 로직 (actionsRes, statusesRes)
- 설문조사 유무 체크 로직

### 기존 코드에서 제거할 것
- 2x2 그리드 레이아웃
- 그리드 카드 스타일 (rounded-xl border 등)
- CATEGORY_ICONS 맵 (리스트 행에서는 다르게 처리)

## 기술 제약
- JavaScript (TypeScript 아님)
- Composition API `<script setup>`
- Tailwind CSS
- 기존 lucide-vue-next 아이콘 사용 가능
- brandColor는 conventionStore에서 가져옴

## 완료 기준
1. `npm run build` 성공
2. `npm run lint` 경고/에러 없음
3. 더보기 화면에 3개 섹션(내 정보, 배정 정보, 메뉴)이 표시됨
4. 배정 정보에 GuestAttribute 키-값이 2열 그리드로 표시됨
5. 속성이 없는 사용자는 배정 정보 섹션이 숨겨짐
6. 설문조사/체크리스트/동적 메뉴 클릭이 정상 작동
7. 동반자 토글이 정상 작동
8. 파일 크기 400줄 이하

## 테스트 요구사항
- 빌드 검증: `cd ClientApp && npm run build && npm run lint`
- 수동 검증: 속성 있는/없는 사용자, 설문 있는/없는 행사, 동반자 있는/없는 경우
