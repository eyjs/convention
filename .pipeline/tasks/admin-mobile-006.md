# Task admin-mobile-006: 알림발송 앱 진입 불가 디버깅

## 목표
Capacitor WebView 환경에서 NotificationSender 페이지 진입이 불가능한 문제의 원인을 분석하고 수정한다. 웹에서는 정상 동작하지만 앱에서만 터치 시 무반응이라는 증상.

## 의존성
- 선행 태스크: 없음 (admin-mobile-003과 독립)
- 외부 의존성: Capacitor WebView 환경 테스트 필요

## 구현 상세

### 조사 대상 파일
1. `ClientApp/src/components/admin/NotificationSender.vue` (274줄)
2. `ClientApp/src/router/index.js` -- NotificationSender 라우트 정의
3. 관리자 사이드바/네비게이션에서 알림발송 메뉴 링크 부분
4. `ClientApp/src/components/admin/AdminSidebar.vue`

### 분석 방향

#### 1. 라우터 설정 확인
- NotificationSender가 어떤 라우트에 매핑되어 있는지 확인
- lazy loading (`() => import(...)`) 사용 시 WebView에서 동적 import 실패 가능성
- 라우트 가드에서 WebView 특수 조건으로 차단되는지 확인

#### 2. 컴포넌트 로딩 방식 확인
- NotificationSender가 직접 라우트 컴포넌트인지, 또는 관리자 탭 시스템 내 서브 컴포넌트인지 확인
- 관리자 페이지가 탭 방식이라면 탭 전환 이벤트가 WebView에서 다르게 동작할 수 있음

#### 3. JavaScript 에러 확인 포인트
- `onMounted`의 `loadData()` 호출에서 API 에러 발생 시 빈 catch `{}` 때문에 조용히 실패
- `Promise.all` 내 API 호출 중 하나가 WebView에서 CORS/권한 문제로 실패하면 전체 페이지 렌더링 차단 가능
- seatingLayouts API 호출이 `.catch(() => ({ data: [] }))` 처리되어 있지만 다른 호출은 미처리

#### 4. 터치 이벤트 확인
- 앱 진입 불가가 "터치 시 무반응"이라면 네비게이션 링크 자체의 문제
- `@click` 이벤트가 WebView에서 동작하는지 확인
- `@touchstart` / `@touchend` 관련 이슈 가능성

#### 5. WebView 호환성
- NotificationSender에서 사용하는 API/기능 중 WebView 미지원 항목 확인
- `seatingLayouts` API가 404/500 반환 시 에러 핸들링 확인

### 수정 방향

가능한 원인별 수정:

**원인 A: loadData() 에러로 렌더링 실패**
- 빈 catch 블록에 `console.error` 추가
- `Promise.all` 대신 개별 try-catch로 분리

**원인 B: 라우트 진입 자체 실패**
- 라우터 에러 핸들링 추가
- `router.onError()` 글로벌 에러 핸들러로 확인

**원인 C: 네비게이션 터치 이벤트 미동작**
- 해당 메뉴 링크에 `@click` 대신 `router-link` 사용 확인
- 터치 영역(min-height 44px) 확보

## 기술 제약
- Vue 3 + JavaScript
- Capacitor WebView 환경 (실기기 또는 에뮬레이터 테스트 필요)
- 프론트엔드만 수정

## 완료 기준
1. 원인 분석 결과를 코드 주석 또는 별도 문서로 기록
2. 수정 후 웹 브라우저에서 정상 동작 유지
3. 가능하다면 앱(Capacitor WebView) 환경에서 진입 확인
4. 에러 핸들링 개선 (빈 catch 블록 제거, console.error 추가)
5. `npm run build` 성공
6. `npm run lint` 경고/에러 없음

## 테스트 요구사항
- 웹 브라우저에서 알림발송 페이지 정상 진입 확인
- 앱(Capacitor) 환경에서 알림발송 메뉴 터치 후 페이지 전환 확인
- Network 탭에서 API 호출 에러 여부 확인
- Console 탭에서 JS 에러 여부 확인
