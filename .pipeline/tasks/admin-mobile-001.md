# Task admin-mobile-001: 참석자관리 모바일 대응 (CRITICAL)

## 목표
GuestManagement.vue의 검색+필터 영역이 모바일에서 1글자만 보이는 문제를 해결하고, 기존 모바일 카드 뷰가 검색/필터와 정상 연동되는지 확인한다.

## 의존성
- 선행 태스크: 없음
- 외부 의존성: 없음

## 구현 상세

### 수정할 파일
- `ClientApp/src/components/admin/GuestManagement.vue` (676줄)

### 수정 내용

#### 1. 검색+필터 영역 레이아웃 변경 (55행 부근)
현재 상태:
```html
<div class="mt-6 mb-4 flex flex-wrap items-center gap-2">
  <input ... class="flex-1 min-w-0 px-3 py-1.5 ..." />
  <select ... class="px-3 py-1.5 ..." />  <!-- 여권 필터 -->
  <select ... class="px-3 py-1.5 ..." />  <!-- 동반자 필터 -->
</div>
```

수정 방향:
- 검색 입력을 모바일에서 `w-full`로 한 줄 독립 배치
- 필터 셀렉트들은 아래 줄로 분리
- PC에서는 기존과 동일하게 한 줄 유지

```html
<div class="mt-6 mb-4 space-y-2 sm:space-y-0 sm:flex sm:flex-wrap sm:items-center sm:gap-2">
  <input ... class="w-full sm:flex-1 sm:min-w-0 px-3 py-1.5 ..." />
  <div class="flex gap-2">
    <select ... class="flex-1 sm:flex-none px-3 py-1.5 ..." />
    <select ... class="flex-1 sm:flex-none px-3 py-1.5 ..." />
  </div>
</div>
```

#### 2. AdminPageHeader 내 버튼 그룹 확인 (7-52행)
- `AdminPageHeader` 컴포넌트의 슬롯에 여러 `AdminButton`이 들어감
- 이 버튼들이 모바일에서 넘치는지 확인
- 필요 시 `AdminPageHeader` 슬롯 영역에 `flex-wrap gap-2` 확인/추가
- AdminPageHeader.vue 자체는 수정하지 않음 (이미 `flex-wrap` 적용 여부 확인 후 필요 시만)

#### 3. 모바일 카드 뷰 검증 (107-136행)
- 이미 `md:hidden` 카드 뷰 존재 -- 검색/필터 결과인 `filteredGuests`를 사용하는지 확인
- 현재 `v-for="guest in filteredGuests"` 사용 중이므로 연동 정상

#### 4. 모바일 카드 뷰에 일정/속성 정보 추가 (현재 이름+그룹+전화+부서만 표시)
- 카드에 일정 배정 상태 뱃지 추가 (예: "코스A" 태그)
- 여권 상태 도트 추가

## 기술 제약
- Vue 3 + JavaScript (TypeScript 아님)
- Tailwind CSS 브레이크포인트만 사용
- PC 레이아웃(md: 이상) 변경 금지
- AdminPageHeader.vue 공통 컴포넌트 직접 수정 최소화

## 완료 기준
1. 모바일(375px)에서 검색 입력이 풀 너비로 표시되어 검색어 입력 가능
2. 여권/동반자 필터가 검색 입력 아래에 별도 줄로 표시
3. sm(640px) 이상에서 기존 한 줄 레이아웃 유지
4. 모바일 카드 뷰에서 검색/필터가 정상 연동
5. `npm run build` 성공
6. `npm run lint` 경고/에러 없음

## 테스트 요구사항
- 375px 뷰포트에서 검색창에 5글자 이상 입력 가능 확인
- 768px 이상에서 기존 PC 레이아웃 변경 없음 확인
- 검색어 입력 후 모바일 카드 뷰 필터링 정상 동작 확인
