# Task 006: 라우터 + 레이아웃 + targetLocations 정리

## 목표
하단 탭 4개 -> 3개로 변경하고, schedule 라우트를 redirect하며, targetLocations 설명을 업데이트한다.

## 의존성
- 선행 태스크: Task 005 (ConventionHome 재작성 완료 후)
- 외부 의존성: 없음

## 구현 상세

### 수정할 파일
1. **`ClientApp/src/layouts/ConventionLayout.vue`** (73-101줄)
2. **`ClientApp/src/router/index.js`** (96-101줄)
3. **`ClientApp/src/schemas/targetLocations.js`** (38-45줄)

### ConventionLayout.vue 수정

navItems에서 일정 탭 제거:
```javascript
// 변경 전 (4탭)
const navItems = computed(() => [
  { path: basePath.value, label: '홈', ... },
  { path: `${basePath.value}/schedule`, label: '일정', ... },  // 제거
  { path: `${basePath.value}/notices`, label: '게시판', ... },
  { path: `${basePath.value}/features`, label: '더보기', ... },
])

// 변경 후 (3탭)
const navItems = computed(() => [
  { path: basePath.value, label: '홈', ... },
  { path: `${basePath.value}/notices`, label: '게시판', ... },
  { path: `${basePath.value}/features`, label: '더보기', ... },
])
```

### router/index.js 수정

MySchedule 라우트를 redirect로 변경:
```javascript
// 변경 전
{
  path: 'schedule',
  name: 'MySchedule',
  component: () => import('@/views/MySchedule.vue'),
  meta: { title: '나의일정', showNav: true },
},

// 변경 후
{
  path: 'schedule',
  redirect: (to) => ({
    path: `/conventions/${to.params.conventionId}`,
  }),
},
```

**주의**: 기존에 `navigateTo('/schedule')`로 이동하는 코드가 있을 수 있으므로 redirect가 필요.

### targetLocations.js 수정

SCHEDULE_CONTENT_TOP의 설명 업데이트:
```javascript
// 변경 전
{
  key: 'SCHEDULE_CONTENT_TOP',
  displayName: 'My Schedule: Below Date Filter',
  description: '"나의 일정" 페이지의 가로 날짜 선택기 아래에 기능이 추가됩니다.',
  page: 'MySchedule.vue',
  allowedCategories: ['BUTTON', 'BANNER', 'CARD'],
},

// 변경 후
{
  key: 'SCHEDULE_CONTENT_TOP',
  displayName: 'Home: Below Date Filter (Timeline)',
  description: '홈 화면의 타임라인 날짜 선택기 아래에 기능이 추가됩니다.',
  page: 'ConventionHome.vue (ScheduleTimeline)',
  allowedCategories: ['BUTTON', 'BANNER', 'CARD'],
},
```

## 기술 제약
- schedule 라우트의 redirect는 conventionId 파라미터를 유지해야 함
- ConventionLayout의 navItems 아이콘 경로는 기존 그대로 유지 (3개만)
- targetLocations.js의 key 값은 변경 금지 (DB에 저장됨)

## 완료 기준
1. `npm run build` 성공
2. `npm run lint` 경고/에러 없음
3. `dotnet build --no-restore` 성공 (백엔드 변경 없지만 전체 검증)
4. 하단 탭이 3개(홈/게시판/더보기)로 표시
5. `/conventions/:id/schedule` 접근 시 `/conventions/:id`로 redirect
6. targetLocations의 SCHEDULE_CONTENT_TOP 설명이 홈 화면 기준으로 업데이트
7. 기존 Dynamic Action 관리자 UI에서 SCHEDULE_CONTENT_TOP 설정이 깨지지 않음

## 테스트 요구사항
- 빌드 검증: `cd ClientApp && npm run build && npm run lint`
- 전체 빌드 검증: `dotnet build --no-restore`
- 수동 검증:
  - 하단 탭 3개 확인
  - `/conventions/1/schedule` URL 직접 접근 시 홈으로 redirect
  - 관리자 > 액션 관리에서 SCHEDULE_CONTENT_TOP 위치가 정상 표시
