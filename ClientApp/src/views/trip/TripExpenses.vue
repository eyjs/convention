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

        <!-- 일정비용 -->
        <div class="bg-white rounded-2xl shadow-md overflow-hidden">
          <button @click="toggleCategory('itinerary')" class="w-full p-5 flex justify-between items-center">
            <div class="flex items-center gap-3">
              <span class="text-2xl">🍽️</span>
              <div class="text-left">
                <h3 class="font-bold text-gray-900">일정비용</h3>
                <p class="text-xs text-gray-500">{{ categoryStats.itinerary.count }}건</p>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <p class="text-xl font-bold text-primary-600">₩{{ categoryStats.itinerary.total.toLocaleString('ko-KR') }}</p>
              <svg class="w-5 h-5 text-gray-400 transition-transform" :class="{ 'rotate-180': expandedCategories.itinerary }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </div>
          </button>

          <div v-if="expandedCategories.itinerary" class="border-t border-gray-100 p-5 pt-3 space-y-2">
            <div v-for="item in trip.itineraryItems?.filter(i => i.expenseAmount > 0)" :key="item.id" class="flex justify-between items-start text-sm">
              <div>
                <p class="font-medium text-gray-900">{{ item.locationName }}</p>
                <p class="text-xs text-gray-500">Day {{ item.dayNumber }}</p>
              </div>
              <p class="font-semibold text-gray-700">₩{{ item.expenseAmount.toLocaleString('ko-KR') }}</p>
            </div>
          </div>
        </div>

        <!-- 숙소비용 -->
        <div class="bg-white rounded-2xl shadow-md overflow-hidden">
          <button @click="toggleCategory('accommodation')" class="w-full p-5 flex justify-between items-center">
            <div class="flex items-center gap-3">
              <span class="text-2xl">🏨</span>
              <div class="text-left">
                <h3 class="font-bold text-gray-900">숙소비용</h3>
                <p class="text-xs text-gray-500">{{ categoryStats.accommodation.count }}건</p>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <p class="text-xl font-bold text-primary-600">₩{{ categoryStats.accommodation.total.toLocaleString('ko-KR') }}</p>
              <svg class="w-5 h-5 text-gray-400 transition-transform" :class="{ 'rotate-180': expandedCategories.accommodation }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </div>
          </button>

          <div v-if="expandedCategories.accommodation" class="border-t border-gray-100 p-5 pt-3 space-y-2">
            <div v-for="item in trip.accommodations?.filter(a => a.expenseAmount > 0)" :key="item.id" class="flex justify-between items-start text-sm">
              <div>
                <p class="font-medium text-gray-900">{{ item.name }}</p>
                <p class="text-xs text-gray-500">{{ formatDate(item.checkInTime) }} ~ {{ formatDate(item.checkOutTime) }}</p>
              </div>
              <p class="font-semibold text-gray-700">₩{{ item.expenseAmount.toLocaleString('ko-KR') }}</p>
            </div>
          </div>
        </div>

        <!-- 교통비용 -->
        <div class="bg-white rounded-2xl shadow-md overflow-hidden">
          <button @click="toggleCategory('transportation')" class="w-full p-5 flex justify-between items-center">
            <div class="flex items-center gap-3">
              <span class="text-2xl">🚗</span>
              <div class="text-left">
                <h3 class="font-bold text-gray-900">교통비용</h3>
                <p class="text-xs text-gray-500">{{ categoryStats.transportation.count }}건</p>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <p class="text-xl font-bold text-primary-600">₩{{ categoryStats.transportation.total.toLocaleString('ko-KR') }}</p>
              <svg class="w-5 h-5 text-gray-400 transition-transform" :class="{ 'rotate-180': expandedCategories.transportation }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </div>
          </button>

          <div v-if="expandedCategories.transportation" class="border-t border-gray-100 p-5 pt-3">
            <button @click="openTransportationModal" class="w-full mb-3 py-2 bg-primary-50 text-primary-600 rounded-lg text-sm font-semibold hover:bg-primary-100 transition-colors">
              + 교통수단 관리
            </button>

            <div class="space-y-2">
              <div v-for="item in trip.flights" :key="item.id" class="flex justify-between items-start text-sm">
                <div>
                  <p class="font-medium text-gray-900">{{ getCategoryIcon(item.category) }} {{ item.category }}</p>
                  <p v-if="item.bookingReference" class="text-xs text-gray-500">예약: {{ item.bookingReference }}</p>
                </div>
                <p class="font-semibold text-gray-700">₩{{ (item.amount || 0).toLocaleString('ko-KR') }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <BottomNavigationBar v-if="tripId" :trip-id="tripId" :show="!uiStore.isModalOpen" />

    <!-- 교통수단 관리 모달 -->
    <FlightManagementModal
      :is-open="isTransportationModalOpen"
      :flights="trip.flights"
      @close="closeTransportationModal"
      @add="handleAddTransportation"
      @edit="handleEditTransportation"
      @delete="handleDeleteTransportation"
    />

    <!-- 교통수단 추가/수정 모달 (TripDetail에서 가져온 것과 동일) -->
    <SlideUpModal :is-open="isTransportationEditModalOpen" @close="closeTransportationEditModal" z-index-class="z-[60]">
      <template #header-title>{{ editingTransportation?.id ? '교통수단 수정' : '교통수단 추가' }}</template>
      <template #body>
        <form id="transportation-form" @submit.prevent="saveTransportation" class="space-y-4">
          <div>
            <label class="label">교통수단</label>
            <input v-model="transportationData.category" type="text" class="input bg-gray-50" readonly />
          </div>

          <!-- 택시는 일정 바인딩 필수 -->
          <div v-if="transportationData.category === '택시'">
            <label class="label">연결된 일정 *</label>
            <select v-model="transportationData.itineraryItemId" class="input" required>
              <option :value="null" disabled>일정을 선택하세요</option>
              <option v-for="item in trip.itineraryItems" :key="item.id" :value="item.id">
                {{ item.dayNumber }}일차 - {{ item.locationName }}
              </option>
            </select>
          </div>

          <!-- 항공편: 예약번호 + 금액 -->
          <template v-if="transportationData.category === '항공편'">
            <div>
              <label class="label">예약번호 (선택)</label>
              <input v-model="transportationData.bookingReference" type="text" class="input" placeholder="예약번호" />
            </div>
            <div>
              <label class="label">금액 (원) *</label>
              <input v-model.number="transportationData.amount" type="number" class="input" placeholder="예: 150000" min="0" step="100" required />
            </div>
          </template>

          <!-- 기차/버스: 금액만 -->
          <template v-else-if="transportationData.category === '기차' || transportationData.category === '버스'">
            <div>
              <label class="label">금액 (원) *</label>
              <input v-model.number="transportationData.amount" type="number" class="input" placeholder="예: 50000" min="0" step="100" required />
            </div>
          </template>

          <!-- 택시: 금액만 -->
          <template v-else-if="transportationData.category === '택시'">
            <div>
              <label class="label">금액 (원) *</label>
              <input v-model.number="transportationData.amount" type="number" class="input" placeholder="예: 10000" min="0" step="100" required />
            </div>
          </template>

          <!-- 렌트카/자가용: 세부 비용 -->
          <template v-else-if="transportationData.category === '렌트카' || transportationData.category === '자가용'">
            <div class="bg-primary-50 border border-primary-200 rounded-lg p-3 mb-2">
              <p class="text-xs text-primary-700">💡 여행 전체 기간 동안 발생한 비용을 입력하세요</p>
            </div>
            <div v-if="transportationData.category === '렌트카'">
              <label class="label">렌트 비용 (원)</label>
              <input v-model.number="transportationData.rentalCost" type="number" class="input" placeholder="예: 100000" min="0" step="100" />
            </div>
            <div>
              <label class="label">주유비 (원)</label>
              <input v-model.number="transportationData.fuelCost" type="number" class="input" placeholder="예: 50000" min="0" step="100" />
            </div>
            <div>
              <label class="label">톨비 (원)</label>
              <input v-model.number="transportationData.tollFee" type="number" class="input" placeholder="예: 20000" min="0" step="100" />
            </div>
            <div>
              <label class="label">주차비 (원)</label>
              <input v-model.number="transportationData.parkingFee" type="number" class="input" placeholder="예: 15000" min="0" step="100" />
            </div>
            <div class="pt-3 border-t">
              <div class="flex justify-between items-center">
                <span class="text-sm font-medium text-gray-700">총 비용:</span>
                <span class="text-lg font-bold text-primary-600">₩{{ calculateTotalTransportationCost().toLocaleString('ko-KR') }}</span>
              </div>
            </div>
          </template>
        </form>
      </template>
      <template #footer>
        <div class="flex gap-3 w-full">
          <button type="button" @click="closeTransportationEditModal" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">취소</button>
          <button type="submit" form="transportation-form" class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all">저장</button>
        </div>
      </template>
    </SlideUpModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import MainHeader from '@/components/common/MainHeader.vue';
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue';
import FlightManagementModal from '@/components/personalTrip/FlightManagementModal.vue';
import SlideUpModal from '@/components/common/SlideUpModal.vue';
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
const isTransportationModalOpen = ref(false);
const isTransportationEditModalOpen = ref(false);
const editingTransportation = ref(null);
const transportationData = ref({});

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
  const itineraryTotal = trip.value.itineraryItems?.reduce((sum, item) => sum + (item.expenseAmount || 0), 0) || 0;
  const accommodationTotal = trip.value.accommodations?.reduce((sum, item) => sum + (item.expenseAmount || 0), 0) || 0;
  const transportationTotal = trip.value.flights?.reduce((sum, item) => sum + (item.amount || 0), 0) || 0;
  return itineraryTotal + accommodationTotal + transportationTotal;
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

  // 숙소비용 (체크인 날짜 기준)
  trip.value.accommodations?.forEach(acc => {
    if (acc.expenseAmount > 0 && acc.checkInTime) {
      const checkInDate = dayjs(acc.checkInTime);
      const startDate = dayjs(trip.value.startDate);
      const dayNumber = checkInDate.diff(startDate, 'day') + 1;
      const day = days.get(dayNumber);
      if (day) {
        day.accommodationExpenses += acc.expenseAmount;
      }
    }
  });

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

// 여행 전체 비용 (항공편, 기차, 버스, 렌트카, 자가용)
const tripWideExpenses = computed(() => {
  const items = [];
  const categories = ['항공편', '기차', '버스', '렌트카', '자가용'];

  categories.forEach(category => {
    const categoryFlights = trip.value.flights?.filter(f => f.category === category) || [];
    const total = categoryFlights.reduce((sum, f) => sum + (f.amount || 0), 0);
    if (total > 0) {
      items.push({ category, amount: total, count: categoryFlights.length });
    }
  });

  const total = items.reduce((sum, item) => sum + item.amount, 0);

  return { items, total };
});

// 카테고리별 통계
const categoryStats = computed(() => {
  const itineraryItems = trip.value.itineraryItems?.filter(i => i.expenseAmount > 0) || [];
  const accommodationItems = trip.value.accommodations?.filter(a => a.expenseAmount > 0) || [];
  const transportationItems = trip.value.flights || [];

  return {
    itinerary: {
      total: itineraryItems.reduce((sum, i) => sum + i.expenseAmount, 0),
      count: itineraryItems.length,
    },
    accommodation: {
      total: accommodationItems.reduce((sum, a) => sum + a.expenseAmount, 0),
      count: accommodationItems.length,
    },
    transportation: {
      total: transportationItems.reduce((sum, f) => sum + (f.amount || 0), 0),
      count: transportationItems.length,
    },
  };
});

// 교통수단 관리
function openTransportationModal() {
  isTransportationModalOpen.value = true;
}

function closeTransportationModal() {
  isTransportationModalOpen.value = false;
}

function handleAddTransportation(category) {
  editingTransportation.value = null;
  transportationData.value = { category };
  isTransportationEditModalOpen.value = true;
}

function handleEditTransportation(transportation) {
  editingTransportation.value = transportation;
  transportationData.value = { ...transportation };
  isTransportationEditModalOpen.value = true;
}

async function handleDeleteTransportation(flightId) {
  if (!confirm('이 교통수단을 삭제하시겠습니까?')) return;
  try {
    await apiClient.delete(`/personal-trips/flights/${flightId}`);
    await loadTrip();
  } catch (error) {
    console.error('Failed to delete transportation:', error);
    alert('삭제에 실패했습니다.');
  }
}

function closeTransportationEditModal() {
  isTransportationEditModalOpen.value = false;
}

function calculateTotalTransportationCost() {
  const toll = transportationData.value.tollFee || 0;
  const fuel = transportationData.value.fuelCost || 0;
  const parking = transportationData.value.parkingFee || 0;
  const rental = transportationData.value.rentalCost || 0;
  return toll + fuel + parking + rental;
}

async function saveTransportation() {
  try {
    // 렌트카/자가용일 경우 세부 비용을 amount에 합산
    if (transportationData.value.category === '렌트카' || transportationData.value.category === '자가용') {
      transportationData.value.amount = calculateTotalTransportationCost();
    }
    // 택시가 아니면 일정 바인딩 제거
    if (transportationData.value.category !== '택시') {
      transportationData.value.itineraryItemId = null;
    }

    const payload = { ...transportationData.value, personalTripId: tripId.value };

    if (editingTransportation.value?.id) {
      await apiClient.put(`/personal-trips/flights/${editingTransportation.value.id}`, payload);
    } else {
      await apiClient.post(`/personal-trips/${tripId.value}/flights`, payload);
    }

    await loadTrip();
    closeTransportationEditModal();
    closeTransportationModal();
  } catch (error) {
    console.error('Failed to save transportation:', error);
    alert('저장에 실패했습니다.');
  }
}

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
  };
  return icons[category] || '🚗';
}

function formatDayDate(dayNumber) {
  if (!trip.value.startDate) return '';
  const date = dayjs(trip.value.startDate).add(dayNumber - 1, 'day');
  return date.format('M/D(ddd)');
}

function formatDate(dateStr) {
  if (!dateStr) return '';
  return dayjs(dateStr).format('M/D');
}

onMounted(() => {
  loadTrip();
});
</script>
