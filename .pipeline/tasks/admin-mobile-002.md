# Task admin-mobile-002: 여행배정 모바일 대응 (CRITICAL)

## 목표
TravelAssignmentManager.vue의 모바일 카드 뷰에 누락된 배정 필드를 추가하고, 일괄배정 입력의 고정 폭을 반응형으로 교체한다.

## 의존성
- 선행 태스크: 없음
- 외부 의존성: 없음

## 구현 상세

### 수정할 파일
- `ClientApp/src/components/admin/TravelAssignmentManager.vue` (578줄)

### 수정 내용

#### 1. 일괄배정 컨트롤 바 반응형 대응 (108-158행)

현재 상태:
```html
<div class="mb-3 p-3 bg-gray-50 rounded-lg flex flex-wrap items-center gap-3">
  <!-- 그룹 필터 -->
  <!-- 구분선 w-px h-6 -->
  <!-- 일괄 입력: w-20, w-28 고정 -->
  <!-- 구분선 w-px h-6 -->
  <!-- 이전 날짜 복사 -->
</div>
```

수정 방향:
- 컨트롤 바를 모바일에서 세로 스택으로 변경
- `w-20` -> `w-full sm:w-20`, `w-28` -> `w-full sm:w-28`
- `gap-3` -> `gap-x-3 gap-y-2`
- 구분선(`w-px h-6`)은 모바일에서 숨김: `hidden sm:block w-px h-6 bg-gray-300`
- 일괄 입력 영역을 모바일에서 별도 줄로

```html
<div class="mb-3 p-3 bg-gray-50 rounded-lg flex flex-wrap items-center gap-x-3 gap-y-2">
  <!-- 그룹 필터: 모바일 풀 너비 -->
  <div class="flex items-center gap-1.5 w-full sm:w-auto">
    ...
    <select ... class="flex-1 sm:flex-none px-2 py-1.5 ..." />
  </div>
  
  <div class="hidden sm:block w-px h-6 bg-gray-300"></div>
  
  <!-- 일괄 입력: 모바일 풀 너비 -->
  <div class="flex items-center gap-1.5 w-full sm:w-auto flex-wrap">
    <span class="text-xs text-gray-500">일괄:</span>
    <input ... class="flex-1 sm:flex-none px-2 py-1.5 ... sm:w-20" />
    <input ... class="flex-1 sm:flex-none px-2 py-1.5 ... sm:w-28" />
    <button ...>적용</button>
  </div>
  
  <div class="hidden sm:block w-px h-6 bg-gray-300"></div>
  
  <!-- 이전 날짜 복사 -->
  ...
</div>
```

#### 2. 모바일 카드 뷰에 룸메이트 필드 추가 (160-190행)

현재 모바일 카드에 있는 필드: 호차, 호텔, 방번호, 메모 (4개 grid-cols-2)
- 현재 구현 상태가 이미 충분하므로, 데이터 불완전이라는 요구사항 확인
- `getDayValue` 함수가 roommate, table 등 추가 필드를 지원하는지 확인
- 지원하는 필드가 있으면 카드에 추가

현재 카드 레이아웃은 `grid grid-cols-2 gap-2`로 4개 필드 -- 충분히 편집 가능한 구조.
요구사항에서 "룸메이트, 버스, 테이블" 언급하나, 실제 데이터 모델에 roommate/table 필드가 있는지 확인 필요.

수정: 카드 상단에 그룹명 외에 추가 정보 표시, 카드 편집 입력은 현행 유지.

#### 3. 날짜 탭 모바일 대응 (79-95행)

현재 `overflow-x-auto` + `whitespace-nowrap` -> 이미 가로 스크롤 가능.
추가 조치: 탭 버튼에 `min-w-fit` 추가하여 터치 영역 확보.

## 기술 제약
- Vue 3 + JavaScript
- Tailwind CSS 브레이크포인트만 사용
- PC 레이아웃(md: 이상) 변경 금지
- 엑셀 업로드 영역은 모바일 대응 제외 (PC 전용)

## 완료 기준
1. 모바일(375px)에서 일괄배정 입력(호차/호텔)이 찌그러지지 않고 풀 너비로 표시
2. 일괄배정 `gap-y`가 적용되어 줄바꿈 시 간격 확보
3. 모바일 카드 뷰에서 호차/호텔/방번호/메모 편집 가능 (기존 유지 확인)
4. 768px 이상에서 기존 PC 테이블 레이아웃 변경 없음
5. `npm run build` 성공
6. `npm run lint` 경고/에러 없음

## 테스트 요구사항
- 375px 뷰포트에서 호차/호텔 입력 필드가 터치 가능한 크기로 표시
- 모바일에서 카드 내 입력 필드 편집 후 저장 정상 동작
- PC(1024px+)에서 기존 테이블 편집 기능 변경 없음 확인
