# 기획자 수정 요청 (2026-04-22)

## 요약
기획자로부터 받은 9건의 버그 수정 및 기능 개선 요청. 참석자(사용자) 화면 UX 개선과 관리자 기능 보완이 주요 내용.

---

## REQ-01: 일정 이미지가 참석자 홈화면에 표시되지 않음 (버그)

### 현상
- 관리자 > 행사상세 > 일정상세 > 사진 업로드 후, 사용자 홈화면 타임라인에 이미지가 표시되지 않음
- 기존에 있던 기능이 사라짐

### 원인 분석
- `ScheduleTimeline.vue`에서 `images: item.images || []` 데이터는 가져오지만 **템플릿에서 렌더링하지 않음**
- 타임라인 카드에 이미지 갤러리 슬롯이 없음

### 요구사항
- 타임라인 각 일정 카드 아래에 이미지 갤러리 표시 (최대 3개 슬롯)
- 관리자 화면의 일정 이미지와 동일한 배치
- 이미지 클릭 시 전체 화면 보기 (기존 `fullImageUrl` 로직 활용)

### 수정 대상
- `ClientApp/src/components/convention/ScheduleTimeline.vue` — 타임라인 카드 내 이미지 렌더링 추가

### 우선순위: **높음** (기존 기능 regression)

---

## REQ-02: 일정 장소 링크가 참석자 화면에서 작동하지 않음 (버그)

### 현상
- 관리자가 일정 업로드 시 링크(mapUrl)를 넣었으나 참석자 화면에서 클릭 불가

### 원인 분석
- `ScheduleTimeline.vue:129` — 장소 텍스트는 표시하지만 **링크(mapUrl) 연결 없음**
- `ScheduleDetailModal.vue:23-31` — 상세 모달에서는 mapUrl 링크 처리됨
- 타임라인 목록에서는 장소를 단순 텍스트로만 표시

### 요구사항
- 타임라인 목록에서도 `mapUrl`이 있는 장소는 클릭 가능한 링크로 표시
- **링크 있는 장소**: 파란색 텍스트 + 클릭 시 링크 열기
- **링크 없는 장소**: 현재 초록색(`#1D9E75`) 유지
- 장소 클릭 시 링크 열기 (일정 카드 클릭과 분리 — `@click.stop` 필요)

### 수정 대상
- `ClientApp/src/components/convention/ScheduleTimeline.vue` — 장소 텍스트에 조건부 링크 추가

### 우선순위: **높음** (기존 기능 미동작)

---

## REQ-03: 일정 상세 모달 하단 잘림 + 스크롤 문제 (버그)

### 현상
- 일정 상세 클릭 시 하단 버튼이 잘려서 보이지 않음
- 모달 내에서 스크롤해야 하는데 뒤 페이지가 스크롤됨

### 원인 분석
- `SlideUpModal.vue` — body 스크롤 차단 (`overflow: hidden`) 적용되어 있으나 불완전할 수 있음
- 모바일에서 `touch-action: none`만으로는 스크롤 전파 완전 차단 안될 수 있음
- 모달 높이가 컨텐츠를 충분히 수용하지 못할 수 있음

### 요구사항
- 모달 내부에서만 스크롤되고 뒤 페이지는 스크롤되지 않아야 함
- 하단 버튼이 항상 보여야 함 (모달 footer 영역 고정)
- 긴 컨텐츠도 모달 내에서 완전히 스크롤 가능

### 수정 대상
- `ClientApp/src/components/common/SlideUpModal.vue` — 스크롤 전파 차단 강화
- `ClientApp/src/components/convention/ScheduleDetailModal.vue` — 하단 영역 고정 처리

### 우선순위: **높음** (사용 불가 수준)

---

## REQ-04: 속성 카테고리 저장 실패 + 여행 가이드 위치 변경

### 04-A: 카테고리 속성키 저장 안됨 (버그)

#### 현상
- 카테고리 추가 → 속성키 선택 → 저장 시 속성키가 저장되지 않음

#### 원인 분석 (확정)
- `AttributeCategoryManager.vue:226-229` — POST 요청(신규 생성)에서 `attributeKeys` 필드 **누락**
- PUT 요청(수정)에서는 `attributeKeys` 포함됨 (223줄)

#### 수정 방법
```javascript
// 226-229줄: attributeKeys 추가
await apiClient.post(`/admin/conventions/${conventionId}/attribute-categories`, {
  name: form.value.name.trim(),
  icon: form.value.icon.trim() || null,
  attributeKeys: form.value.attributeKeys,  // 이 줄 추가
})
```

#### 수정 대상
- `ClientApp/src/components/admin/AttributeCategoryManager.vue` — POST 요청에 attributeKeys 포함

#### 우선순위: **높음** (기능 미동작)

### 04-B: 여행 가이드를 "더보기" 메뉴로 이동 (기능 변경)

#### 현상
- 홈 상단에 여행 가이드 영역이 나오는데 공간을 차지함

#### 요구사항
- 홈(`ConventionHome.vue`)에서 여행 가이드 링크 제거
- 더보기(`MoreFeaturesView.vue`) 메뉴 섹션에 여행 가이드 메뉴 항목 추가
- 별도 섹션으로 배치 (기존 설문/체크리스트와 같은 패턴)

#### 수정 대상
- `ClientApp/src/views/ConventionHome.vue` — 여행 가이드 링크 블록 제거
- `ClientApp/src/views/MoreFeaturesView.vue` — 여행 가이드 메뉴 항목 추가

#### 우선순위: **중간**

---

## REQ-05: 홈 날짜 필터 고정 + 연락처 전화걸기

### 05-A: 날짜 필터 스크롤 시 고정 (UI 개선)

#### 현상
- 홈에서 스크롤하면 날짜 필터도 같이 스크롤됨

#### 원인 분석
- `ScheduleTimeline.vue:4` — 날짜 스크롤 컨테이너가 `relative`만 적용, **sticky 없음**

#### 요구사항
- 날짜 필터 영역을 `sticky top-0`으로 고정 (헤더 아래 고정)
- 스크롤해도 날짜 선택 UI가 항상 보여야 함

#### 수정 대상
- `ClientApp/src/components/convention/ScheduleTimeline.vue` — 날짜 필터 컨테이너에 `sticky` 추가

#### 우선순위: **중간**

### 05-B: 연락처 전화걸기 (기능 추가)

#### 요구사항
- 더보기 메뉴의 연락처 또는 여행 가이드의 긴급연락처 클릭 시 `tel:` 링크로 전화걸기

#### 수정 대상
- `ClientApp/src/views/TravelGuide.vue` — 연락처에 `<a href="tel:번호">` 적용

#### 우선순위: **중간**

---

## REQ-06: 게시판 카테고리 뱃지 색상/아이콘 등록 (기능 추가)

### 현상
- 게시판 카테고리 등록 시 색상과 아이콘을 선택할 수 없음

### 현재 상태
- `BoardManagement.vue` — 카테고리 생성 폼에 이름, 설명, 표시 순서만 있음
- 색상(`categoryColor`)은 게시글 단위로 사용되고 있음

### 요구사항
- 카테고리 생성/수정 시 뱃지 색상 선택기 추가
- 카테고리별 아이콘 선택 기능 추가
- 게시판 목록에서 카테고리 뱃지에 선택한 색상/아이콘 표시

### 수정 대상
- **백엔드**: 카테고리 엔티티에 `Color`, `Icon` 필드 추가 (DB 마이그레이션)
- `ClientApp/src/components/admin/BoardManagement.vue` — 카테고리 폼에 색상/아이콘 선택기
- `ClientApp/src/views/Board.vue` — 카테고리 뱃지 렌더링에 색상/아이콘 적용

### 우선순위: **낮음** (신규 기능)

---

## REQ-07: 게시판 UI 개선 (UI 수정)

### 요구사항
- 게시글 카드의 상하 여백 줄이기
- 세로 가운데 정렬
- 눈 아이콘(조회수)을 목록에서 제거, 게시글 상세 안에서만 표시

### 수정 대상
- `ClientApp/src/views/Board.vue` — 게시글 카드 패딩 축소, 조회수 아이콘 제거
- 게시글 상세 뷰 — 조회수 표시 유지/추가

### 우선순위: **낮음** (UI 개선)

---

## REQ-08: SMS 그룹별 발송 기능 (기능 확인/보완)

### 현재 상태
- `NotificationSender.vue` — ALL / GROUP / INDIVIDUAL 선택 가능
- 그룹 선택 UI 존재 (`form.targetScope === 'GROUP'` → 그룹 드롭다운)

### 확인 필요
- 현재 그룹별 발송이 실제로 동작하는지 확인
- `SmsManagementModal.vue`에서도 그룹 선택이 가능한지 확인
- 만약 SmsManagementModal에 그룹 선택이 없다면 추가 필요

### 수정 대상
- `ClientApp/src/components/admin/sms/SmsManagementModal.vue` — 그룹 선택 기능 확인/추가

### 우선순위: **중간**

---

## REQ-09: 참석자 상세에서 속성 수정 기능 (기능 추가)

### 현상
- 참석자 상세 모달에서 속성은 보기/삭제만 가능, 수정 불가

### 요구사항
- 속성 값을 직접 수정할 수 있는 인라인 편집 또는 수정 모달
- 수정 클릭 시 속성별 입력 필드 표시
- 저장 시 API 호출로 속성 업데이트

### 수정 대상
- **백엔드**: 속성 수정 API 확인/추가 (PUT `/admin/guests/{id}/attributes`)
- `ClientApp/src/components/admin/guest/GuestDetailModal.vue` — 속성 수정 UI 추가

### 우선순위: **중간**

---

## 우선순위 정리

| 순서 | ID | 내용 | 우선순위 | 유형 |
|------|-----|------|----------|------|
| 1 | REQ-04A | 카테고리 속성키 저장 버그 | 높음 | 버그 (1줄 수정) |
| 2 | REQ-01 | 일정 이미지 홈화면 미표시 | 높음 | 버그/복원 |
| 3 | REQ-02 | 일정 장소 링크 미동작 | 높음 | 버그/개선 |
| 4 | REQ-03 | 일정 상세 모달 스크롤 | 높음 | 버그 |
| 5 | REQ-05A | 홈 날짜 필터 고정 | 중간 | UI |
| 6 | REQ-04B | 여행 가이드 더보기 이동 | 중간 | 기능 변경 |
| 7 | REQ-05B | 연락처 전화걸기 | 중간 | 기능 추가 |
| 8 | REQ-07 | 게시판 UI 여백/조회수 | 낮음 | UI |
| 9 | REQ-08 | SMS 그룹별 발송 확인 | 중간 | 확인/보완 |
| 10 | REQ-09 | 참석자 속성 수정 | 중간 | 기능 추가 |
| 11 | REQ-06 | 게시판 카테고리 색상/아이콘 | 낮음 | 신규 기능 |

---

## 기술적 참고사항

### 영향받는 핵심 파일
1. `ClientApp/src/components/convention/ScheduleTimeline.vue` — REQ-01, 02, 05A
2. `ClientApp/src/components/convention/ScheduleDetailModal.vue` — REQ-03
3. `ClientApp/src/components/common/SlideUpModal.vue` — REQ-03
4. `ClientApp/src/components/admin/AttributeCategoryManager.vue` — REQ-04A
5. `ClientApp/src/views/ConventionHome.vue` — REQ-04B
6. `ClientApp/src/views/MoreFeaturesView.vue` — REQ-04B, 05B
7. `ClientApp/src/views/Board.vue` — REQ-06, 07
8. `ClientApp/src/components/admin/BoardManagement.vue` — REQ-06
9. `ClientApp/src/components/admin/sms/SmsManagementModal.vue` — REQ-08
10. `ClientApp/src/components/admin/guest/GuestDetailModal.vue` — REQ-09
11. `ClientApp/src/views/TravelGuide.vue` — REQ-05B

### DB 마이그레이션 필요
- REQ-06: 게시판 카테고리에 Color, Icon 컬럼 추가
- REQ-09: 속성 수정 API가 없는 경우 백엔드 추가

### 프론트엔드만 수정 (무중단 배포 가능)
- REQ-01, 02, 03, 04A, 04B, 05A, 05B, 07
