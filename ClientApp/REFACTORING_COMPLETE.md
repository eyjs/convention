# 프론트엔드 리팩토링 완료 보고서

## ✅ 완료된 작업

### 1. 레이아웃 시스템

- ✅ `App.vue` - 동적 레이아웃 로더 구현
- ✅ `layouts/DefaultLayout.vue` - 메인 앱 레이아웃
- ✅ `layouts/FeatureLayout.vue` - 동적 기능 전용 레이아웃

### 2. 라우터 구조

- ✅ `router/index.js` - 모든 라우트에 `meta.layout` 추가
- ✅ `router/dynamic.js` - 동적 기능 라우트 정의
- ✅ `/features` - 기능 목록 페이지 라우트
- ✅ `/feature/:featureName` - 동적 기능 라우트

### 3. 상태 관리 및 서비스

- ✅ `stores/feature.js` - 동적 기능 상태 관리
- ✅ `services/featureService.js` - Feature API 서비스

### 4. 동적 기능 시스템

- ✅ `dynamic-features/DynamicFeatureLoader.vue` - 동적 컴포넌트 로더
- ✅ `views/MoreFeaturesView.vue` - 기능 목록 페이지
- ✅ `dynamic-features/TestFeature/` - 테스트 기능 예시

### 5. 백엔드 수정

- ✅ `Models/Feature.cs` - 새 구조로 변경
- ✅ `Controllers/FeaturesController.cs` - CRUD API 구현
- ✅ `migrations/migrate_feature_model.sql` - 마이그레이션 스크립트

## 📝 다음 단계

### 1. 데이터베이스 마이그레이션

```bash
cd C:/Users/USER/dev/startour/convention/migrations
# SQL Server에서 migrate_feature_model.sql 실행
```

### 2. 테스트 데이터 추가

```sql
INSERT INTO Features (ConventionId, MenuName, MenuUrl, IsActive, IconUrl)
VALUES (1, '테스트 기능', 'test', 1, '/icons/test.svg');
```

### 3. 프론트엔드 테스트

1. `npm run dev`
2. 로그인 후 하단 네비 "더보기" 클릭
3. `/features` 페이지에서 기능 목록 확인
4. "테스트 기능" 클릭 → `/feature/test`
5. TestPage 렌더링 확인

### 4. 기존 파일 이동 (점진적)

```
views/ → features/
components/ → features/ 또는 shared/
```

### 5. 실제 동적 기능 구현

- SurveyFeature (설문조사)
- VotingFeature (투표)
- GameFeature (미니게임)

## 🔧 새 동적 기능 추가 방법

### 1. 백엔드 등록

```sql
INSERT INTO Features (ConventionId, MenuName, MenuUrl, IsActive, IconUrl)
VALUES (1, '설문조사', 'survey', 1, '/icons/survey.svg');
```

### 2. 프론트엔드 컴포넌트 생성

```
dynamic-features/
  SurveyFeature/
    views/
      SurveyPage.vue
```

### 3. 컴포넌트 구조

```vue
<template>
  <div class="survey-page">
    <h2>설문조사</h2>
    <!-- 기능 구현 -->
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  featureMetadata: Object,
})

onMounted(() => {
  // 초기화
})

onUnmounted(() => {
  // 정리
})
</script>

<style scoped>
/* 스타일 격리 */
</style>
```

## 🎯 네이밍 규칙

| 항목       | 형식                 | 예시                |
| ---------- | -------------------- | ------------------- |
| DB MenuUrl | kebab-case           | `survey`, `my-game` |
| 폴더명     | PascalCase + Feature | `SurveyFeature`     |
| 파일명     | PascalCase + Page    | `SurveyPage.vue`    |
| URL        | kebab-case           | `/feature/survey`   |

## 🚨 주의사항

1. **스타일 격리**: 항상 `<style scoped>` 사용
2. **상태 정리**: `onUnmounted`에서 타이머/구독 정리
3. **에러 처리**: 비동기 작업에 try-catch 필수
4. **URL 검증**: MenuUrl은 kebab-case만 허용

## 📦 파일 구조

```
ClientApp/src/
├── App.vue (수정됨)
├── layouts/ (새로 생성)
│   ├── DefaultLayout.vue
│   └── FeatureLayout.vue
├── router/ (수정됨)
│   ├── index.js
│   └── dynamic.js
├── stores/ (추가됨)
│   └── feature.js
├── services/ (추가됨)
│   └── featureService.js
├── views/ (추가됨)
│   └── MoreFeaturesView.vue
└── dynamic-features/ (새로 생성)
    ├── DynamicFeatureLoader.vue
    └── TestFeature/
        └── views/
            └── TestPage.vue
```

## ✨ 주요 기능

### 동적 레이아웃 전환

```javascript
// App.vue
const currentLayout = computed(() => {
  const layoutName = route.meta.layout
  if (!layoutName) return null
  return defineAsyncComponent(() => import(`@/layouts/${layoutName}.vue`))
})
```

### 동적 컴포넌트 로딩

```javascript
// DynamicFeatureLoader.vue
const featureComponent = computed(() => {
  const formattedName = formatFeatureName(props.featureName)
  return defineAsyncComponent({
    loader: () =>
      import(
        `@/dynamic-features/${formattedName}Feature/views/${formattedName}Page.vue`
      ),
  })
})
```

### 기능 목록 표시

```javascript
// MoreFeaturesView.vue
onMounted(async () => {
  const conventionId = localStorage.getItem('selectedConventionId')
  await featureStore.fetchActiveFeatures(conventionId)
})
```

## 🔗 API 엔드포인트

| Method | Endpoint                                            | 설명           |
| ------ | --------------------------------------------------- | -------------- |
| GET    | `/api/conventions/{id}/features`                    | 기능 목록 조회 |
| GET    | `/api/conventions/{id}/features/{featureId}`        | 기능 상세      |
| POST   | `/api/conventions/{id}/features`                    | 기능 생성      |
| PUT    | `/api/conventions/{id}/features/{featureId}`        | 기능 수정      |
| PATCH  | `/api/conventions/{id}/features/{featureId}/status` | 활성화 토글    |
| DELETE | `/api/conventions/{id}/features/{featureId}`        | 기능 삭제      |

## 🐛 트러블슈팅

### 1. 동적 기능이 로드되지 않음

- MenuUrl과 폴더/파일명 확인
- 브라우저 콘솔에서 에러 확인
- 네트워크 탭에서 API 응답 확인

### 2. 레이아웃이 적용되지 않음

- `route.meta.layout` 값 확인
- 레이아웃 파일 경로 확인

### 3. 하단 네비가 표시되지 않음

- `route.meta.showNav` 기본값은 true
- false로 설정된 라우트 확인

## 📚 참고 자료

- Vue Router: https://router.vuejs.org/
- Pinia: https://pinia.vuejs.org/
- defineAsyncComponent: https://vuejs.org/api/general.html#defineasynccomponent
