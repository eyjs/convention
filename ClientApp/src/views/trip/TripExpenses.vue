<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader :title="'가계부'" :show-back="true">
      <template #actions>
        <button @click="showExportMenu = true" class="p-2 text-gray-500 hover:bg-gray-100 rounded-lg">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
          </svg>
        </button>
      </template>
    </MainHeader>

    <div v-if="loading" class="text-center py-20">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
      <p class="mt-4 text-gray-600 font-medium">가계부를 불러오는 중...</p>
    </div>

    <div v-else class="max-w-2xl mx-auto px-4 py-4 pb-24">
      <!-- 총 지출 헤더 -->
      <div class="bg-gradient-to-r from-primary-600 to-primary-400 rounded-2xl shadow-xl p-6 mb-4 text-white">
        <p class="text-sm opacity-90 mb-1">총 지출</p>
        <h1 class="text-4xl font-bold">₩{{ totalExpenses.toLocaleString('ko-KR') }}</h1>
      </div>

      <!-- 뷰 전환 탭 -->
      <div class="flex gap-2 mb-4">
        <button
          @click="currentView = 'daily'"
          :class="currentView === 'daily' ? 'bg-primary-500 text-white' : 'bg-white text-gray-700'"
          class="flex-1 py-3 rounded-xl font-semibold shadow-md transition-all"
        >
          📅 일자별
        </button>
        <button
          @click="currentView = 'category'"
          :class="currentView === 'category' ? 'bg-primary-500 text-white' : 'bg-white text-gray-700'"
          class="flex-1 py-3 rounded-xl font-semibold shadow-md transition-all"
        >
          📊 카테고리별
        </button>
      </div>

      <!-- 일자별 뷰 -->
      <div v-if="currentView === 'daily'" class="space-y-4">
        <!-- 일자별 카드 -->
        <div v-for="day in dailyExpenses" :key="day.dayNumber" class="bg-white rounded-2xl shadow-md p-5">
          <div class="flex justify-between items-start mb-3">
            <div>
              <h3 class="text-lg font-bold text-gray-900">Day {{ day.dayNumber }}</h3>
              <p class="text-xs text-gray-500">{{ formatDayDate(day.dayNumber) }}</p>
            </div>
            <p class="text-2xl font-bold text-primary-600">₩{{ day.total.toLocaleString('ko-KR') }}</p>
          </div>

          <div class="space-y-2">
            <div v-if="day.itineraryExpenses > 0" class="flex justify-between items-center text-sm">
              <span class="text-gray-600">🍽️ 일정</span>
              <span class="font-semibold text-gray-900">₩{{ day.itineraryExpenses.toLocaleString('ko-KR') }} <span class="text-xs text-gray-400">({{ day.itineraryCount }}건)</span></span>
            </div>
            <div v-if="day.accommodationExpenses > 0" class="flex justify-between items-center text-sm">
              <span class="text-gray-600">🏨 숙소</span>
              <span class="font-semibold text-gray-900">₩{{ day.accommodationExpenses.toLocaleString('ko-KR') }}</span>
            </div>
            <div v-if="day.transportationExpenses > 0" class="flex justify-between items-center text-sm">
              <span class="text-gray-600">🚕 교통 (택시)</span>
              <span class="font-semibold text-gray-900">₩{{ day.transportationExpenses.toLocaleString('ko-KR') }} <span class="text-xs text-gray-400">({{ day.taxiCount }}건)</span></span>
            </div>
          </div>
        </div>

        <!-- 여행 전체 비용 카드 -->
        <div v-if="tripWideExpenses.total > 0" class="bg-white rounded-2xl shadow-md p-5 border-2 border-primary-200">
          <div class="flex justify-between items-start mb-3">
            <div>
              <h3 class="text-lg font-bold text-gray-900">여행 전체 비용</h3>
              <p class="text-xs text-gray-500">기간 전체에 걸친 비용</p>
            </div>
            <p class="text-2xl font-bold text-primary-600">₩{{ tripWideExpenses.total.toLocaleString('ko-KR') }}</p>
          </div>

          <div class="space-y-2">
            <div v-for="item in tripWideExpenses.items" :key="item.category" class="flex justify-between items-center text-sm">
              <span class="text-gray-600">{{ getCategoryIcon(item.category) }} {{ item.category }}</span>
              <span class="font-semibold text-gray-900">₩{{ item.amount.toLocaleString('ko-KR') }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 카테고리별 뷰 -->
      <div v-if="currentView === 'category'" class="space-y-4">
        <!-- 도넛 차트 영역 (나중에 구현) -->
        <div class="bg-white rounded-2xl shadow-md p-6">
          <div class="text-center py-8">
            <p class="text-2xl mb-2">📊</p>
            <p class="text-sm text-gray-500">차트는 향후 구현 예정</p>
          </div>
        </div>

        <div v-for="category in categoryStats" :key="category.key" class="bg-white rounded-2xl shadow-md overflow-hidden">
          <button @click="toggleCategory(category.key)" class="w-full p-5 flex justify-between items-center">
            <div class="flex items-center gap-3">
              <span class="text-2xl">{{ category.icon }}</span>
              <div class="text-left">
                <h3 class="font-bold text-gray-900">{{ category.name }}</h3>
                <p class="text-xs text-gray-500">{{ category.count }}건</p>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <p class="text-xl font-bold text-primary-600">₩{{ category.total.toLocaleString('ko-KR') }}</p>
              <svg class="w-5 h-5 text-gray-400 transition-transform" :class="{ 'rotate-180': expandedCategories[category.key] }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </div>
          </button>

          <div v-if="expandedCategories[category.key]" class="border-t border-gray-100 p-5 pt-3 space-y-2">
            <!-- Content for each category type -->
            <template v-if="category.key === 'itinerary'">
              <div v-for="item in trip.itineraryItems?.filter(i => i.expenseAmount > 0)" :key="item.id" class="flex justify-between items-start text-sm">
                <div>
                  <p class="font-medium text-gray-900">{{ item.locationName }}</p>
                  <p class="text-xs text-gray-500">Day {{ item.dayNumber }}</p>
                </div>
                <p class="font-semibold text-gray-700">₩{{ item.expenseAmount.toLocaleString('ko-KR') }}</p>
              </div>
            </template>
            <template v-else-if="category.key === 'accommodation'">
              <div v-for="item in trip.accommodations?.filter(a => a.expenseAmount > 0)" :key="item.id" class="flex justify-between items-start text-sm">
                <div>
                  <p class="font-medium text-gray-900">{{ item.name }}</p>
                  <p class="text-xs text-gray-500">{{ formatDate(item.checkInTime) }} ~ {{ formatDate(item.checkOutTime) }}</p>
                </div>
                <p class="font-semibold text-gray-700">₩{{ item.expenseAmount.toLocaleString('ko-KR') }}</p>
              </div>
            </template>
                        <template v-else-if="category.key === 'transportation'">
                          <div v-for="item in trip.flights" :key="item.id" class="flex justify-between items-start text-sm">
                            <div>
                              <p class="font-medium text-gray-900">{{ getCategoryIcon(item.category) }} {{ item.category }}</p>
                              <p v-if="item.bookingReference" class="text-xs text-gray-500">예약: {{ item.bookingReference }}</p>
                            </div>
                            <p class="font-semibold text-gray-700">₩{{ (item.amount || 0).toLocaleString('ko-KR') }}</p>
                          </div>
                        </template>
          </div>
        </div>
      </div>
    </div>

    <BottomNavigationBar v-if="tripId" :trip-id="tripId" :show="!uiStore.isModalOpen" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import MainHeader from '@/components/common/MainHeader.vue';
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue';
import { useUIStore } from '@/stores/ui';
import apiClient from '@/services/api';
import dayjs from 'dayjs';

const uiStore = useUIStore();
const route = useRoute();
const tripId = computed(() => route.params.id);

const loading = ref(true);
const trip = ref({});
const currentView = ref('daily'); // 'daily' or 'category'
const expandedCategories = ref({
  itinerary: false,
  accommodation: false,
  transportation: false,
});

const showExportMenu = ref(false);

// 데이터 로드
async function loadTrip() {
  try {
    loading.value = true;
    const response = await apiClient.get(`/personal-trips/${tripId.value}`);
    trip.value = response.data;
  } catch (error) {
    console.error('Failed to load trip:', error);
    alert('여행 정보를 불러오는데 실패했습니다.');
  } finally {
    loading.value = false;
  }
}

// 총 지출 계산
const totalExpenses = computed(() => {
  let total = 0
  if (trip.value.itineraryItems) {
    total += trip.value.itineraryItems.reduce((sum, item) => sum + (item.expenseAmount || 0), 0)
  }
  if (trip.value.accommodations) {
    total += trip.value.accommodations.reduce((sum, acc) => sum + (acc.expenseAmount || 0), 0)
  }
  if (trip.value.flights) {
    total += trip.value.flights.reduce((sum, flight) => {
      if (flight.category === '렌트카' || flight.category === '자가용') {
        return sum + (flight.amount || 0);
      } else {
        return sum + (flight.amount || 0) + (flight.tollFee || 0) +
               (flight.fuelCost || 0) + (flight.parkingFee || 0);
      }
    }, 0)
  }
  return total
});

// 일자별 통계
const dailyExpenses = computed(() => {
  if (!trip.value.itineraryItems) return [];

  const days = new Map();
  const numDays = Math.max(...trip.value.itineraryItems.map(i => i.dayNumber), 0);

  // 각 일자 초기화
  for (let i = 1; i <= numDays; i++) {
    days.set(i, {
      dayNumber: i,
      itineraryExpenses: 0,
      itineraryCount: 0,
      accommodationExpenses: 0,
      transportationExpenses: 0,
      taxiCount: 0,
      total: 0,
    });
  }

  // 일정비용
  trip.value.itineraryItems?.forEach(item => {
    if (item.expenseAmount > 0) {
      const day = days.get(item.dayNumber);
      if (day) {
        day.itineraryExpenses += item.expenseAmount;
        day.itineraryCount++;
      }
    }
  });

  // 숙소비용 (체크인 날짜 기준) - dailyExpenses에서 제외
  // trip.value.accommodations?.forEach(acc => {
  //   if (acc.expenseAmount > 0 && acc.checkInTime) {
  //     const checkInDate = dayjs(acc.checkInTime);
  //     const startDate = dayjs(trip.value.startDate);
  //     const dayNumber = checkInDate.diff(startDate, 'day') + 1;
  //     const day = days.get(dayNumber);
  //     if (day) {
  //       day.accommodationExpenses += acc.expenseAmount;
  //     }
  //   }
  // });

  // 교통비용 (택시만 일자별)
  trip.value.flights?.filter(f => f.category === '택시' && f.itineraryItemId).forEach(taxi => {
    const itinerary = trip.value.itineraryItems?.find(i => i.id === taxi.itineraryItemId);
    if (itinerary) {
      const day = days.get(itinerary.dayNumber);
      if (day) {
        day.transportationExpenses += taxi.amount || 0;
        day.taxiCount++;
      }
    }
  });

  // 총액 계산
  days.forEach(day => {
    day.total = day.itineraryExpenses + day.accommodationExpenses + day.transportationExpenses;
  });

  return Array.from(days.values()).filter(day => day.total > 0);
});

// 여행 전체 비용 (항공편, 기차, 버스, 렌트카, 자가용, 숙소)
const tripWideExpenses = computed(() => {
  const items = [];
  const categories = ['항공편', '기차', '버스', '렌트카', '자가용'];

  // 교통 비용
  categories.forEach(category => {
    const categoryFlights = trip.value.flights?.filter(f => f.category === category) || [];
    const total = categoryFlights.reduce((sum, f) => sum + (f.amount || 0), 0);
    if (total > 0) {
      items.push({ category, amount: total, count: categoryFlights.length });
    }
  });

  // 숙소 비용
  const accommodationTotal = trip.value.accommodations?.reduce((sum, acc) => sum + (acc.expenseAmount || 0), 0) || 0;
  if (accommodationTotal > 0) {
    items.push({ category: '숙소', amount: accommodationTotal, count: trip.value.accommodations?.length || 0 });
  }

  const total = items.reduce((sum, item) => sum + item.amount, 0);

  return { items, total };
});

// 카테고리별 통계
const categoryStats = computed(() => {
  const itineraryItems = trip.value.itineraryItems?.filter(i => i.expenseAmount > 0) || [];
  const accommodationItems = trip.value.accommodations?.filter(a => a.expenseAmount > 0) || [];
  const transportationItems = trip.value.flights || [];

  const stats = [
    {
      name: '일정비용',
      key: 'itinerary',
      total: itineraryItems.reduce((sum, i) => sum + i.expenseAmount, 0),
      count: itineraryItems.length,
      icon: '🍽️', // Added icon for display
    },
    {
      name: '숙소비용',
      key: 'accommodation',
      total: accommodationItems.reduce((sum, a) => sum + (a.expenseAmount || 0), 0),
      count: accommodationItems.length,
      icon: '🏨', // Added icon for display
    },
    {
      name: '교통비용',
      key: 'transportation',
      total: transportationItems.reduce((sum, f) => sum + (f.amount || 0), 0),
      count: transportationItems.length,
      icon: '🚗', // Added icon for display
    },
  ];

  // 비용 내림차순으로 정렬
  return stats.sort((a, b) => b.total - a.total);
});

// 유틸리티 함수
function toggleCategory(category) {
  expandedCategories.value[category] = !expandedCategories.value[category];
}

function getCategoryIcon(category) {
  const icons = {
    '항공편': '✈️',
    '기차': '🚂',
    '버스': '🚌',
    '택시': '🚕',
    '렌트카': '🚗',
    '자가용': '🚙',
    '숙소': '🏨', // Added accommodation icon
  };
  return icons[category] || '🏠'; // Default to '🏠' (house) instead of car
}

function formatDayDate(dayNumber) {
  if (!trip.value.startDate) return '';
  const date = dayjs(trip.value.startDate).add(dayNumber - 1, 'day');
  const weekdays = ['일', '월', '화', '수', '목', '금', '토']; // Korean weekdays
  return `${date.format('M/D')}(${weekdays[date.day()]})`;
}

function formatDate(dateStr) {
  if (!dateStr) return '';
  return dayjs(dateStr).format('M/D');
}

function openTransportationModal() {
  emit('open-flight-modal'); // Assuming the parent TripDetail.vue handles this
}

onMounted(() => {
  loadTrip();
});
</script>
