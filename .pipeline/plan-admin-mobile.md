# 실행 계획: 관리자 페이지 모바일 대응

## 요구사항 요약
관리자 페이지 13개 화면을 모바일에서 사용 가능하도록 개선한다. 현장에서 모바일로 룸번호 수정, 일정 변경, 알림 발송 등을 처리하는 시나리오가 핵심이다. PC 레이아웃은 변경하지 않고 Tailwind 브레이크포인트(`sm:`, `md:`, `lg:`)로 모바일 전용 스타일만 추가한다.

## 기술적 분석

### 영향 범위
- 프론트엔드만 수정 (백엔드 변경 없음)
- 대상 파일: `ClientApp/src/components/admin/` 하위 10개 컴포넌트 + 1개 SMS 모달
- 새 컴포넌트 생성 없음 (기존 컴포넌트에 반응형 클래스 추가)

### 의존성
- 공통 UI 컴포넌트: `AdminTable.vue`, `AdminPageHeader.vue`, `AdminButton.vue` 등
- `AdminTable.vue`는 `hidden md:block` 패턴과 함께 사용 (모바일 카드 뷰는 각 컴포넌트에서 직접 구현)
- 이미 모바일 카드 뷰가 있는 파일: `GuestManagement.vue`, `TravelAssignmentManager.vue`, `BoardManagement.vue`

### 리스크
- NotificationSender 앱(Capacitor WebView) 진입 불가 문제는 프론트엔드 코드만으로 원인 파악이 어려울 수 있음
- OptionTourSurveyManagement가 `AdminTable` 공통 컴포넌트를 사용 중 - 모바일 카드 뷰 추가 시 AdminTable과 별도로 구현 필요

### 선행 조건
- 없음 (모든 태스크가 프론트엔드 Tailwind 클래스 수정이므로 독립 실행 가능)

## 아키텍처 결정

| 결정 | 근거 |
|------|------|
| 새 컴포넌트 생성하지 않음 | 기존 컴포넌트에 `hidden md:block` / `md:hidden` 패턴으로 분기 |
| AdminTable 수정하지 않음 | 공통 컴포넌트 변경 시 영향 범위가 넓어짐, 각 화면에서 개별 대응 |
| 모바일 카드 뷰는 `md:hidden` 블록으로 추가 | 기존 GuestManagement, BoardManagement 패턴과 일관성 유지 |
| `flex-wrap` + `gap` 패턴 우선 적용 | 필터/버튼 영역은 wrap만으로 대부분 해결 가능 |

## 구현 전략
1. 기존 PC 레이아웃 코드는 건드리지 않고 `md:` / `lg:` 접두사만 추가
2. 모바일 카드 뷰가 필요한 테이블은 `md:hidden` 블록을 테이블 위에 추가
3. 고정 폭(`w-20`, `w-28`, `w-96`)은 `w-full sm:w-20` 패턴으로 교체
4. `whitespace-nowrap`은 `md:whitespace-nowrap`으로 교체 (모바일에서 줄바꿈 허용)

## 단계별 실행 계획

### Phase 1: CRITICAL (사용 불가 해결)
- **목표**: 참석자관리, 여행배정 모바일에서 정상 사용 가능
- **태스크**: admin-mobile-001, admin-mobile-002

### Phase 2: MAJOR - 일정/알림 (현장 필수)
- **목표**: 일정관리, 알림발송, 액션관리 모바일 대응
- **태스크**: admin-mobile-003

### Phase 3: MAJOR - SMS/게시판
- **목표**: SMS 발송, 게시판관리 모바일 대응
- **태스크**: admin-mobile-004

### Phase 4: MINOR 일괄
- **목표**: 옵션투어, 폼빌더, 대시보드, 인쇄미리보기 미세 조정
- **태스크**: admin-mobile-005

### Phase 5: 알림발송 앱 진입 불가 디버깅
- **목표**: Capacitor WebView에서 NotificationSender 진입 불가 원인 분석 및 수정
- **태스크**: admin-mobile-006

## 예상 태스크 목록

| 태스크 | 이름 | 의존성 | 대상 파일 수 | 복잡도 |
|--------|------|--------|-------------|--------|
| admin-mobile-001 | 참석자관리 모바일 대응 | 없음 | 1 | 중 |
| admin-mobile-002 | 여행배정 모바일 대응 | 없음 | 1 | 중 |
| admin-mobile-003 | 일정/알림/액션 관리 모바일 대응 | 없음 | 3 | 중 |
| admin-mobile-004 | SMS/게시판 관리 모바일 대응 | 없음 | 2 | 중 |
| admin-mobile-005 | MINOR 화면 일괄 모바일 대응 | 없음 | 4 | 하 |
| admin-mobile-006 | 알림발송 앱 진입 불가 디버깅 | 없음 | 1+ | 불확실 |
