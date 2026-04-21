# Task admin-mobile-005: MINOR 화면 일괄 모바일 대응

## 목표
OptionTourSurveyManagement, FormBuilderEditor, DashboardOverview, TablePrint 4개 화면의 사소한 모바일 UX 문제를 일괄 수정한다.

## 의존성
- 선행 태스크: 없음
- 외부 의존성: 없음

## 구현 상세

### 수정할 파일
1. `ClientApp/src/components/admin/OptionTourSurveyManagement.vue` (237줄)
2. `ClientApp/src/components/admin/FormBuilderEditor.vue` (514줄)
3. `ClientApp/src/components/admin/DashboardOverview.vue` (504줄)
4. `ClientApp/src/components/admin/TablePrint.vue` (840줄)

---

### 파일 1: OptionTourSurveyManagement.vue

#### 1-1. 모바일 카드 뷰 추가 (68행 부근)

현재 `AdminTable` 컴포넌트만 사용하여 테이블 표시.
모바일에서 테이블이 가로 스크롤됨.

수정:
- `AdminTable` 래퍼에 `hidden md:block` 추가
- 위에 `md:hidden` 모바일 카드 뷰 추가

```html
<!-- 모바일 카드 뷰 -->
<div class="md:hidden space-y-2">
  <div
    v-for="survey in filteredSurveys"
    :key="'m-' + survey.id"
    class="bg-white rounded-lg border p-3"
  >
    <div class="flex items-center justify-between mb-1">
      <span class="font-medium text-gray-900 text-sm">{{ survey.title }}</span>
      <AdminBadge :variant="getSurveyStatusVariant(survey)">
        {{ getSurveyStatusLabel(survey) }}
      </AdminBadge>
    </div>
    <div class="text-xs text-gray-500 space-y-0.5">
      <p>응답: {{ survey.responseCount ?? 0 }}명</p>
      <p>기간: {{ formatDateRange(survey) }}</p>
      <p>생성: {{ formatDate(survey.createdAt) }}</p>
    </div>
    <div class="mt-2 flex gap-3 text-xs">
      <button class="text-primary-600" @click="showEditView(survey.id)">수정</button>
      <button class="text-green-600" @click="showStatsView(survey.id)">통계</button>
      <button class="text-red-600" @click="confirmDeleteSurvey(survey)">삭제</button>
    </div>
  </div>
</div>

<!-- PC 테이블 -->
<div class="hidden md:block">
  <AdminTable ...> ... </AdminTable>
</div>
```

---

### 파일 2: FormBuilderEditor.vue

#### 2-1. 그리드 gap 반응형 (41행)

현재 상태:
```html
<div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
```

수정:
```html
<div class="grid grid-cols-1 lg:grid-cols-3 gap-3 lg:gap-6">
```

---

### 파일 3: DashboardOverview.vue

#### 3-1. 상단 통계 카드 그리드 (3행)

현재 상태:
```html
<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mb-8">
```

이미 `grid-cols-1 sm:grid-cols-2` 적용. 양호.

#### 3-2. 여권 현황 요약 카드 (54행)

현재 상태:
```html
<div class="grid grid-cols-2 sm:grid-cols-5 gap-3 mb-4">
```

320px에서 `grid-cols-2`가 좁을 수 있음.
수정: `grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3 mb-4`

또는 카드 내부 패딩을 `p-2 sm:p-3`으로 축소하여 320px에서도 2열 유지.

---

### 파일 4: TablePrint.vue

#### 4-1. 사이드바 고정 폭 반응형 (7행)

현재 상태:
```html
<aside class="no-print w-full md:w-96 bg-white border-r ...">
```

이미 `w-full md:w-96` 적용되어 있음. 모바일에서 풀 너비, PC에서 384px 고정.
수정 불필요 -- 요구사항의 `w-96` -> `w-full md:w-96`가 이미 적용된 상태.

## 기술 제약
- Vue 3 + JavaScript
- Tailwind CSS 브레이크포인트만 사용
- PC 레이아웃 변경 금지
- AdminTable.vue 공통 컴포넌트 수정 금지

## 완료 기준
1. OptionTourSurveyManagement: 모바일에서 투어명+날짜+참석자수가 카드로 표시
2. FormBuilderEditor: 모바일에서 gap이 `gap-3`으로 적용
3. DashboardOverview: 320px에서 여권 통계 카드가 찌그러지지 않음
4. TablePrint: 기존 `w-full md:w-96` 확인 (이미 적용됨)
5. `npm run build` 성공
6. `npm run lint` 경고/에러 없음

## 테스트 요구사항
- OptionTourSurveyManagement: 375px에서 카드 뷰 정상 표시, 수정/통계/삭제 버튼 터치 가능
- FormBuilderEditor: 375px에서 필드 팔레트와 편집 영역 세로 배치 확인
- DashboardOverview: 320px 뷰포트에서 통계 카드 텍스트 잘림 없음 확인
