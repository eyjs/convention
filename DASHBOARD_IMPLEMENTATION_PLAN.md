# 여행 대시보드 구현 작업계획서

## 📋 프로젝트 개요
- **목적**: 여행의 모든 정보를 한눈에 파악할 수 있는 대시보드 페이지 구현
- **위치**: `/trips/:id/dashboard` (홈 화면)
- **사용자**: 여행을 등록한 사용자
- **디자인 컨셉**: 초록색 테마 (primary-500 계열), 모바일 최적화

---

## 🎯 대시보드 구성 요소

### 1. 히어로 섹션
- 여행 제목, 날짜, D-day 표시
- 여행 전/중/후 상태 구분

### 2. 오늘의 요약 (여행 중에만 표시)
- 현재 진행중인 일정 표시
- 다음 일정 미리보기
- 오늘 일정 개수 및 지출 요약

### 3. 카테고리 카드 (2x2 그리드)
- 일정 카드: 총 개수, 완료/미완료, [+ 일정 추가], [> 전체보기]
- 숙소 카드: 예약 개수, 다음 체크인, [+ 숙소 추가], [> 상세보기]
- 교통 카드: 교통수단별 개수, [+ 교통 추가], [> 예약정보]
- 가계부 카드: 총 지출, 예산, 사용률 %, [> 상세보기]

### 4. 지출 요약 (가로 막대 차트)
- 카테고리별 지출 금액 및 비율
- 총 지출 / 예산 / 잔여 금액 표시

### 5. 알림 & 리마인더
- 체크인/아웃, 항공편, 렌트카 반납 등 중요 일정 알림
- 긴급도별 색상 구분 (🔴 24시간, 🟡 48시간, 🟢 일반)

---

## 🔧 구현 작업 목록

### Phase 1: 백엔드 - 예산 기능 추가

#### 1-1. 데이터베이스 스키마 변경
**파일**: `Entities/PersonalTrip/PersonalTrip.cs`
```csharp
// 추가할 필드
[Column(TypeName = "decimal(18, 2)")]
public decimal? Budget { get; set; }
```

#### 1-2. 마이그레이션 생성 및 적용
```bash
dotnet ef migrations add AddBudgetToPersonalTrip --no-build
dotnet ef database update --no-build
```

#### 1-3. DTO 업데이트
**파일들**:
- `DTOs/PersonalTrip/PersonalTripDto.cs`
- `DTOs/PersonalTrip/CreatePersonalTripDto.cs`
- `DTOs/PersonalTrip/UpdatePersonalTripDto.cs`

```csharp
// 각 DTO에 추가
public decimal? Budget { get; set; }
```

#### 1-4. 서비스 레이어 매핑 업데이트
**파일**: `Services/PersonalTrip/PersonalTripService.cs`

**CreatePersonalTripAsync**:
```csharp
Budget = dto.Budget,
```

**UpdatePersonalTripAsync**:
```csharp
trip.Budget = dto.Budget;
```

**MapToDto**:
```csharp
Budget = trip.Budget,
```

---

### Phase 2: 프론트엔드 - 예산 입력 UI

#### 2-1. 여행 정보 입력 모달에 예산 필드 추가
**파일**: `ClientApp/src/views/trip/TripDetail.vue`

**템플릿 수정** (line ~92 근처):
```vue
<div>
  <label class="block text-sm font-medium text-gray-700 mb-1">예산 (선택)</label>
  <input
    v-model.number="tripData.budget"
    type="number"
    class="w-full input"
    placeholder="예: 1000000"
    min="0"
    step="10000"
  />
  <p class="text-xs text-gray-500 mt-1">예산을 입력하면 대시보드에서 지출률을 확인할 수 있습니다.</p>
</div>
```

**스크립트 수정**:
- `tripData` ref에 `budget` 필드 추가 확인
- `saveTripInfo()` 함수에서 budget 전송 확인

---

### Phase 3: 프론트엔드 - 대시보드 페이지 생성

#### 3-1. 대시보드 컴포넌트 생성
**파일**: `ClientApp/src/views/trip/TripDashboard.vue`

**전체 구조**:
```vue
<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader :title="'대시보드'" :show-back="true" />

    <div v-if="loading" class="text-center py-20">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto"></div>
      <p class="mt-4 text-gray-600 font-medium">여행 정보를 불러오는 중...</p>
    </div>

    <div v-else class="max-w-2xl mx-auto px-4 py-4 pb-24">
      <!-- 1. 히어로 섹션 -->
      <section class="bg-gradient-to-r from-primary-600 to-primary-400 rounded-2xl shadow-xl p-6 mb-4 text-white">
        <!-- 구현 내용 -->
      </section>

      <!-- 2. 오늘의 요약 (여행 중에만) -->
      <section v-if="isTripOngoing" class="bg-white rounded-2xl shadow-md p-5 mb-4">
        <!-- 구현 내용 -->
      </section>

      <!-- 3. 카테고리 카드 그리드 -->
      <section class="grid grid-cols-2 gap-4 mb-4">
        <!-- 4개 카드 -->
      </section>

      <!-- 4. 지출 요약 -->
      <section class="bg-white rounded-2xl shadow-md p-5 mb-4">
        <!-- 구현 내용 -->
      </section>

      <!-- 5. 알림 & 리마인더 -->
      <section class="bg-white rounded-2xl shadow-md p-5">
        <!-- 구현 내용 -->
      </section>
    </div>

    <BottomNavigationBar v-if="tripId" :trip-id="tripId" :show="!uiStore.isModalOpen" />
  </div>
</template>

<script setup>
// imports 및 로직 구현
</script>
```

#### 3-2. 필요한 Computed Properties

```javascript
// 여행 상태 계산
const tripStatus = computed(() => {
  const today = dayjs();
  const start = dayjs(trip.value.startDate);
  const end = dayjs(trip.value.endDate);

  if (today.isBefore(start)) return 'before'; // 출발 전
  if (today.isAfter(end)) return 'after'; // 여행 종료
  return 'ongoing'; // 여행 중
});

const isTripOngoing = computed(() => tripStatus.value === 'ongoing');

// D-day 계산
const dDay = computed(() => {
  const today = dayjs();
  const start = dayjs(trip.value.startDate);
  return start.diff(today, 'day');
});

// 현재 Day 계산 (여행 중일 때)
const currentDay = computed(() => {
  if (!isTripOngoing.value) return null;
  const today = dayjs();
  const start = dayjs(trip.value.startDate);
  return today.diff(start, 'day') + 1;
});

// 현재 진행중인 일정
const currentItinerary = computed(() => {
  if (!isTripOngoing.value) return null;
  const now = new Date();
  // 현재 시간 기준으로 진행중인 일정 찾기
  // trip.value.itineraryItems에서 필터링
});

// 다음 일정
const nextItinerary = computed(() => {
  if (!isTripOngoing.value) return null;
  // 현재 일정 이후의 다음 일정 찾기
});

// 오늘의 일정 개수
const todayItineraryCount = computed(() => {
  if (!isTripOngoing.value) return 0;
  const day = currentDay.value;
  return trip.value.itineraryItems?.filter(i => i.dayNumber === day).length || 0;
});

// 오늘의 지출
const todayExpenses = computed(() => {
  if (!isTripOngoing.value) return 0;
  const day = currentDay.value;
  const items = trip.value.itineraryItems?.filter(i => i.dayNumber === day) || [];
  return items.reduce((sum, item) => sum + (item.expenseAmount || 0), 0);
});

// 총 지출
const totalExpenses = computed(() => {
  const itinerary = trip.value.itineraryItems?.reduce((sum, i) => sum + (i.expenseAmount || 0), 0) || 0;
  const accommodation = trip.value.accommodations?.reduce((sum, a) => sum + (a.expenseAmount || 0), 0) || 0;
  const transportation = trip.value.flights?.reduce((sum, f) => sum + (f.amount || 0), 0) || 0;
  return itinerary + accommodation + transportation;
});

// 예산 사용률
const budgetUsagePercent = computed(() => {
  if (!trip.value.budget || trip.value.budget === 0) return 0;
  return Math.round((totalExpenses.value / trip.value.budget) * 100);
});

// 예산 상태 색상
const budgetStatusColor = computed(() => {
  const percent = budgetUsagePercent.value;
  if (percent >= 90) return 'red'; // 빨간색
  if (percent >= 70) return 'yellow'; // 노란색
  return 'green'; // 초록색
});

// 카테고리별 지출
const expensesByCategory = computed(() => {
  const itinerary = trip.value.itineraryItems?.reduce((sum, i) => sum + (i.expenseAmount || 0), 0) || 0;
  const accommodation = trip.value.accommodations?.reduce((sum, a) => sum + (a.expenseAmount || 0), 0) || 0;
  const transportation = trip.value.flights?.reduce((sum, f) => sum + (f.amount || 0), 0) || 0;
  const total = itinerary + accommodation + transportation;

  return [
    {
      name: '일정',
      amount: itinerary,
      percent: total > 0 ? Math.round((itinerary / total) * 100) : 0,
      color: 'bg-blue-500'
    },
    {
      name: '숙소',
      amount: accommodation,
      percent: total > 0 ? Math.round((accommodation / total) * 100) : 0,
      color: 'bg-green-500'
    },
    {
      name: '교통',
      amount: transportation,
      percent: total > 0 ? Math.round((transportation / total) * 100) : 0,
      color: 'bg-purple-500'
    },
  ];
});

// 알림 목록
const reminders = computed(() => {
  const alerts = [];
  const now = dayjs();

  // 체크인/아웃 알림
  trip.value.accommodations?.forEach(acc => {
    if (acc.checkInTime) {
      const checkIn = dayjs(acc.checkInTime);
      const hoursDiff = checkIn.diff(now, 'hour');
      if (hoursDiff > 0 && hoursDiff <= 24) {
        alerts.push({
          type: 'urgent',
          icon: '🔴',
          message: `체크인 ${hoursDiff}시간 전 (${checkIn.format('M/D HH:mm')})`,
        });
      }
    }
    if (acc.checkOutTime) {
      const checkOut = dayjs(acc.checkOutTime);
      const hoursDiff = checkOut.diff(now, 'hour');
      if (hoursDiff > 0 && hoursDiff <= 24) {
        alerts.push({
          type: 'urgent',
          icon: '🔴',
          message: `체크아웃 ${hoursDiff}시간 전 (${checkOut.format('M/D HH:mm')})`,
        });
      }
    }
  });

  // 항공편 알림
  // ... 추가 로직

  return alerts;
});

// 일정 완료 개수
const completedItineraryCount = computed(() => {
  // TODO: 완료 상태 필드가 있다면 사용, 없으면 과거 일정으로 판단
  return 0;
});
```

#### 3-3. 카테고리 카드 클릭 핸들러

```javascript
// 일정
const handleItineraryCardClick = () => {
  router.push(`/trips/${tripId.value}/itinerary`);
};

const handleAddItinerary = () => {
  // 일정 추가 모달 열기 (또는 itinerary 페이지로 이동)
  router.push(`/trips/${tripId.value}/itinerary`);
};

// 숙소
const handleAccommodationCardClick = () => {
  // 숙소 관리 모달 열기
  isAccommodationManagementModalOpen.value = true;
};

const handleAddAccommodation = () => {
  // 숙소 추가 모달 열기
  isAccommodationEditModalOpen.value = true;
};

// 교통
const handleTransportationCardClick = () => {
  // 교통 관리 모달 열기
  isTransportationModalOpen.value = true;
};

const handleAddTransportation = () => {
  // 교통 추가 모달 열기
  isTransportationModalOpen.value = true;
};

// 가계부
const handleExpensesCardClick = () => {
  router.push(`/trips/${tripId.value}/expenses`);
};
```

---

### Phase 4: 라우팅 설정

#### 4-1. 라우터 설정
**파일**: `ClientApp/src/router/index.js`

```javascript
{
  path: '/trips/:id/dashboard',
  name: 'TripDashboard',
  component: () => import('@/views/trip/TripDashboard.vue'),
  meta: { requiresAuth: true }
}
```

#### 4-2. BottomNavigationBar 수정
**파일**: `ClientApp/src/components/common/BottomNavigationBar.vue`

현재 "홈" 탭이 `/trips/:id`로 가는 경우, `/trips/:id/dashboard`로 변경:
```javascript
const tabs = [
  {
    name: '홈',
    path: `/trips/${props.tripId}/dashboard`, // 변경
    icon: HomeIcon
  },
  // ... 나머지
];
```

---

### Phase 5: 스타일링 및 반응형

#### 5-1. 카드 그리드 레이아웃
```vue
<div class="grid grid-cols-2 gap-4">
  <div class="bg-white rounded-2xl shadow-md p-4 flex flex-col">
    <!-- 카드 내용 -->
    <div class="flex-1">
      <!-- 정보 -->
    </div>
    <div class="mt-4 space-y-2">
      <button class="w-full py-2 bg-primary-50 text-primary-600 rounded-lg text-sm font-semibold">
        + 추가
      </button>
      <button class="w-full py-2 bg-gray-100 text-gray-700 rounded-lg text-sm font-semibold">
        > 상세보기
      </button>
    </div>
  </div>
</div>
```

#### 5-2. 지출 차트 막대
```vue
<div v-for="category in expensesByCategory" :key="category.name" class="mb-3">
  <div class="flex justify-between text-sm mb-1">
    <span>{{ category.name }}</span>
    <span class="font-semibold">{{ category.amount.toLocaleString() }}원 ({{ category.percent }}%)</span>
  </div>
  <div class="w-full bg-gray-200 rounded-full h-3">
    <div
      :class="category.color"
      class="h-3 rounded-full transition-all"
      :style="{ width: category.percent + '%' }"
    ></div>
  </div>
</div>
```

#### 5-3. 예산 사용률 표시
```vue
<div class="flex items-center gap-2">
  <span class="text-sm text-gray-600">사용률</span>
  <span
    class="text-lg font-bold"
    :class="{
      'text-red-600': budgetStatusColor === 'red',
      'text-yellow-600': budgetStatusColor === 'yellow',
      'text-primary-600': budgetStatusColor === 'green',
    }"
  >
    {{ budgetUsagePercent }}%
  </span>
  <span v-if="budgetStatusColor === 'red'">⚠️</span>
</div>
```

---

## 📝 작업 순서

1. ✅ **백엔드 - 예산 필드 추가** (Phase 1)
   - Entity 수정
   - Migration 생성/적용
   - DTO 업데이트
   - Service 매핑

2. ✅ **프론트엔드 - 예산 입력 UI** (Phase 2)
   - TripDetail.vue 모달 수정

3. ✅ **프론트엔드 - 대시보드 생성** (Phase 3)
   - TripDashboard.vue 생성
   - Computed properties 구현
   - 핸들러 함수 구현

4. ✅ **라우팅 설정** (Phase 4)
   - 라우터 추가
   - BottomNav 수정

5. ✅ **스타일링 및 테스트** (Phase 5)
   - 반응형 확인
   - 색상 테마 적용
   - 실제 데이터로 테스트

---

## 🎨 디자인 가이드

### 색상 규칙
- **Primary (초록)**: `bg-primary-500`, `text-primary-600`, `border-primary-200`
- **Success (초록)**: 예산 70% 미만
- **Warning (노랑)**: 예산 70-90%
- **Danger (빨강)**: 예산 90% 이상
- **Gray**: 취소, 닫기 버튼

### 간격
- 섹션 간: `mb-4` (16px)
- 카드 내부 패딩: `p-4` 또는 `p-5`
- 버튼 간격: `space-y-2`

### 폰트 크기
- 섹션 제목: `text-lg font-bold`
- 카드 제목: `text-base font-semibold`
- 본문: `text-sm`
- 보조 정보: `text-xs text-gray-500`

---

## ⚠️ 주의사항

1. **데이터 없을 때 처리**
   - 일정/숙소/교통 없을 때: "아직 등록된 항목이 없습니다" 안내
   - 예산 미설정 시: "예산을 설정하세요" 안내

2. **날짜/시간 계산**
   - dayjs 라이브러리 사용
   - 시간대 고려 (로컬 시간 기준)

3. **성능 최적화**
   - computed 적극 활용
   - 불필요한 재계산 방지

4. **에러 처리**
   - API 호출 실패 시 fallback UI
   - 로딩 상태 표시

---

## 🧪 테스트 체크리스트

- [ ] 출발 전 여행: D-day 정확히 표시
- [ ] 여행 중: 오늘의 요약 카드 표시
- [ ] 여행 종료: "여행 완료" 표시
- [ ] 예산 미설정: 안내 메시지
- [ ] 예산 90% 이상: 빨간색 경고
- [ ] 지출 차트: 비율 정확히 계산
- [ ] 카드 클릭: 올바른 페이지/모달 이동
- [ ] 알림: 24시간/48시간 기준 정확
- [ ] 반응형: 모바일/태블릿 확인

---

## 📚 참고 파일

- `ClientApp/src/views/trip/TripExpenses.vue` (가계부 로직 참고)
- `ClientApp/src/views/trip/TripItinerary.vue` (일정 로직 참고)
- `ClientApp/src/views/trip/TripDetail.vue` (모달 관리 참고)
- `ClientApp/src/components/common/BottomNavigationBar.vue` (네비게이션)

---

## 🔗 관련 이슈 및 문서

- Primary 색상: `rgba(23, 177, 133, 1)` - 초록색 테마
- 취소/닫기 버튼: 회색
- 저장/수정 버튼: 초록색
