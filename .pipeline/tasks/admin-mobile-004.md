# Task admin-mobile-004: SMS/게시판 관리 모바일 대응 (MAJOR)

## 목표
SmsManagementModal과 BoardManagement의 모바일 레이아웃을 개선한다.

## 의존성
- 선행 태스크: 없음
- 외부 의존성: 없음

## 구현 상세

### 수정할 파일
1. `ClientApp/src/components/admin/sms/SmsManagementModal.vue` (704줄)
2. `ClientApp/src/components/admin/BoardManagement.vue` (810줄)

---

### 파일 1: SmsManagementModal.vue

#### 1-1. 모달 너비 모바일 대응 (19행 부근)

현재 상태:
```html
<div class="inline-block ... sm:max-w-3xl sm:w-full">
```

`sm:max-w-3xl`이므로 모바일에서는 이미 풀 너비. 양호.

#### 1-2. 수신자 목록 max-height 반응형

현재 `max-h-[200px]`로 20명 이상 시 스크롤이 부족할 수 있음.
수정: `max-h-[200px]` -> `max-h-[30vh] sm:max-h-[200px]`

수신자 목록을 찾아서 해당 클래스 수정. Step 2(수신자 선택)에서 수신자 체크박스 목록의 max-height를 조정.

#### 1-3. 발송이력 영역 확인

SmsManagementModal 내에 발송이력 테이블이 있는지 확인.
- 있다면 모바일 카드 뷰 패턴(`md:hidden` 카드 + `hidden md:block` 테이블) 추가
- 이력 항목: 발송일시 + 수신자수 + 상태 + 내용 미리보기

#### 1-4. 스텝 표시 모바일 대응 (26-45행)

현재 3단계 스텝 인디케이터가 헤더에 `flex items-center gap-1 text-sm`로 표시.
모바일에서 충분히 작으므로 수정 불필요.

---

### 파일 2: BoardManagement.vue

#### 2-1. 모바일 카드 뷰 확인 (72-89행)

현재 이미 `md:hidden` 모바일 카드 뷰 존재:
```html
<div v-else class="md:hidden divide-y divide-gray-100">
```
카드에 제목 + 카테고리 + 날짜 + 조회수 표시됨. 양호.

#### 2-2. PC 테이블의 whitespace-nowrap 교체 (92행 이후)

PC 테이블(`hidden md:block`) 내 `whitespace-nowrap` 확인.
- 제목 컬럼: `whitespace-nowrap` -> 제거 또는 `md:whitespace-nowrap` (PC에서만)
- 실제로 PC 테이블은 `hidden md:block` 안에 있으므로 모바일에서 보이지 않음
- 따라서 PC 테이블 내 `whitespace-nowrap`은 그대로 유지해도 무방

#### 2-3. 카드 뷰에 제목 truncate 확인

현재 카드에서:
```html
<p class="font-medium text-gray-900 truncate">{{ notice.title }}</p>
```
`truncate`는 1줄 말줄임 -- 모바일에서 적절. 유지.

#### 2-4. 통계 카드 그리드 (40-58행)

현재 `grid grid-cols-1 md:grid-cols-3 gap-4` -- 모바일 1열, PC 3열. 양호.

#### 2-5. 게시글 작성/상세 모달 확인

`NoticeFormModal`, `NoticeDetailModal`이 있는지 확인.
- 이들이 `BaseModal`을 사용하면 이미 모바일 대응됨
- 추가 수정 불필요 예상

## 기술 제약
- Vue 3 + JavaScript
- Tailwind CSS 브레이크포인트만 사용
- PC 레이아웃 변경 금지

## 완료 기준
1. SmsManagementModal: 모바일에서 수신자 목록이 충분한 높이(30vh)로 스크롤 가능
2. SmsManagementModal: 발송이력이 있다면 모바일에서 카드 형태로 가독성 확보
3. BoardManagement: 모바일 카드 뷰에서 제목+카테고리+날짜 정상 표시 (기존 확인)
4. 768px 이상에서 기존 PC 레이아웃 유지
5. `npm run build` 성공
6. `npm run lint` 경고/에러 없음

## 테스트 요구사항
- SmsManagementModal: 375px에서 수신자 30명 목록 스크롤 정상 동작
- BoardManagement: 375px에서 게시글 10개 카드 뷰 정상 표시
- PC(1024px+)에서 기존 레이아웃 변경 없음 확인
