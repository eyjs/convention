# Task admin-mobile-003: 일정/알림/액션 관리 모바일 대응 (MAJOR)

## 목표
ScheduleManagement, NotificationSender, ActionManagement 3개 화면의 모바일 레이아웃을 개선한다.

## 의존성
- 선행 태스크: 없음
- 외부 의존성: 없음

## 구현 상세

### 수정할 파일
1. `ClientApp/src/components/admin/ScheduleManagement.vue` (1025줄)
2. `ClientApp/src/components/admin/NotificationSender.vue` (274줄)
3. `ClientApp/src/components/admin/ActionManagement.vue` (1650줄)

---

### 파일 1: ScheduleManagement.vue

#### 1-1. 코스 필터 버튼 반응형 (44-71행)

현재 상태:
```html
<div class="mb-4 overflow-x-auto scrollbar-hide -mx-1 px-1">
  <div class="flex gap-2 pb-2">
    <button class="flex-shrink-0 px-3 sm:px-4 py-2 rounded-full ... whitespace-nowrap" />
  </div>
</div>
```

수정 방향:
- `flex gap-2` -> `flex flex-wrap gap-2` (모바일에서 줄바꿈 허용)
- `flex-shrink-0` 제거 (wrap 시 축소 허용)
- `overflow-x-auto` -> `sm:overflow-x-auto` 또는 제거 (wrap으로 대체)
- 또는 `grid grid-cols-2 sm:flex gap-2` 패턴 적용

```html
<div class="mb-4">
  <div class="flex flex-wrap gap-2">
    <button class="px-3 sm:px-4 py-2 rounded-full ... whitespace-nowrap" />
  </div>
</div>
```

#### 1-2. 탭 버튼 모바일 대응 (4-27행)

현재 `flex gap-1`으로 2개 탭 -- 모바일에서도 문제 없음. 추가 수정 불필요.

#### 1-3. 일정 카드/목록 확인 (74행 이후)

현재 `space-y-4`로 세로 스택 -- 이미 모바일 친화적. 카드 내부 버튼이 터치 가능한지만 확인.
- 카드 내 편집/삭제 버튼에 `min-h-[44px] min-w-[44px]` 터치 타겟 확보 (필요 시)

---

### 파일 2: NotificationSender.vue

#### 2-1. 타입 필터 그리드 (10행)

현재 상태:
```html
<div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-2">
```

이미 반응형 그리드가 적용되어 있음. 현재 코드가 요구사항의 수정 내용과 일치.
추가 수정 불필요 -- 확인만.

#### 2-2. 발송 폼 세로 스택 확인

현재 `space-y-4`로 세로 배치 -- 모바일 친화적. 수정 불필요.

#### 2-3. 대상 선택 라디오 버튼 (63-67행)

현재 `flex gap-3` -> 모바일에서 3개 라디오가 한 줄에 들어감.
- `flex flex-wrap gap-3`으로 변경하여 좁은 화면에서 줄바꿈 허용

#### 2-4. 발송 이력 카드 확인 (100-128행)

현재 이미 카드 형태 (`border rounded-lg p-4`)로 구현. 모바일 대응 양호.
- `flex items-center justify-between`이 좁은 화면에서 겹치지 않는지 확인
- 필요 시 `flex-wrap` 추가

---

### 파일 3: ActionManagement.vue

#### 3-1. 액션 카드 그리드 반응형 (35행)

현재 상태:
```html
<div class="grid gap-4 mt-6">
```

단일 컬럼 그리드 -- 이미 모바일 친화적. 수정 불필요.

#### 3-2. 카드 내부 레이아웃 (38-79행 부근)

현재 상태:
```html
<div class="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-4">
```

이미 `flex-col` + `lg:flex-row` 반응형. 양호.

#### 3-3. BehaviorType 필터 버튼 (11-25행)

현재 `flex flex-wrap gap-2` -- 이미 wrap 적용. 양호.

#### 3-4. 카드 내 텍스트 truncate -> line-clamp (요구사항 #4)

- 카드 제목에 `break-words` 이미 적용됨
- 설명 텍스트에 `truncate` 사용하는 곳이 있으면 `line-clamp-2`로 교체
- ConfigJson 표시 영역 확인

#### 3-5. 설정 폼 모달 모바일 풀 너비

- 액션 생성/수정 모달이 있는지 확인하여 모바일에서 `w-full` 적용
- `BaseModal` 사용 시 이미 모바일 대응되어 있을 수 있음 -- 확인

## 기술 제약
- Vue 3 + JavaScript
- Tailwind CSS 브레이크포인트만 사용
- PC 레이아웃 변경 금지

## 완료 기준
1. ScheduleManagement: 모바일에서 코스 필터 버튼이 줄바꿈되어 모두 보임
2. NotificationSender: 모바일에서 타입 필터 `grid-cols-2`로 정상 표시 (이미 적용 확인)
3. ActionManagement: 카드 텍스트가 모바일에서 잘리지 않고 `line-clamp` 적용
4. 3개 파일 모두 768px 이상에서 기존 PC 레이아웃 유지
5. `npm run build` 성공
6. `npm run lint` 경고/에러 없음

## 테스트 요구사항
- ScheduleManagement: 375px에서 코스 필터 5개 이상일 때 줄바꿈 확인
- NotificationSender: 375px에서 6개 타입 버튼이 2열 그리드로 표시 확인
- ActionManagement: 375px에서 카드 텍스트 잘림 없이 표시 확인
