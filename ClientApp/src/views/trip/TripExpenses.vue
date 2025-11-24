<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader :title="'가계부'" :show-back="true">
      <template #actions>
        <button @click="showExportMenu = !showExportMenu" class="p-2 text-gray-500 hover:bg-gray-100 rounded-lg relative">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
          </svg>
        </button>
        <!-- Export Menu Dropdown -->
        <div v-if="showExportMenu" class="absolute right-4 top-14 bg-white rounded-xl shadow-lg border z-50 overflow-hidden">
          <button @click="exportToPDF" class="w-full px-4 py-3 text-left hover:bg-gray-50 flex items-center gap-2">
            <svg class="w-5 h-5 text-red-500" fill="currentColor" viewBox="0 0 24 24"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8l-6-6zm-1 2l5 5h-5V4zM6 20V4h6v6h6v10H6z"/></svg>
            PDF 다운로드
          </button>
          <button @click="exportToExcel" class="w-full px-4 py-3 text-left hover:bg-gray-50 flex items-center gap-2 border-t">
            <svg class="w-5 h-5 text-green-600" fill="currentColor" viewBox="0 0 24 24"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8l-6-6zm-1 2l5 5h-5V4zM6 20V4h6v6h6v10H6z"/></svg>
            Excel 다운로드
          </button>
        </div>
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
          일자별
        </button>
        <button
          @click="currentView = 'category'"
          :class="currentView === 'category' ? 'bg-primary-500 text-white' : 'bg-white text-gray-700'"
          class="flex-1 py-3 rounded-xl font-semibold shadow-md transition-all"
        >
          카테고리별
        </button>
      </div>

      <!-- 일자별 뷰 -->
      <div v-if="currentView === 'daily'" class="space-y-4">
        <div v-for="day in dailyExpensesDetailed" :key="day.dayNumber" class="bg-white rounded-2xl shadow-md overflow-hidden">
          <!-- 일자 헤더 -->
          <button @click="toggleDay(day.dayNumber)" class="w-full p-4 flex justify-between items-center">
            <div>
              <h3 class="text-lg font-bold text-gray-900">Day {{ day.dayNumber }}</h3>
              <p class="text-xs text-gray-500">{{ formatDayDate(day.dayNumber) }}</p>
            </div>
            <div class="flex items-center gap-2">
              <p class="text-xl font-bold text-primary-600">₩{{ day.total.toLocaleString('ko-KR') }}</p>
              <svg class="w-5 h-5 text-gray-400 transition-transform" :class="{ 'rotate-180': expandedDays.includes(day.dayNumber) }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </div>
          </button>

          <!-- 일자별 상세 내역 -->
          <div v-if="expandedDays.includes(day.dayNumber)" class="border-t border-gray-100 p-4 space-y-4">
            <!-- 일정 비용 -->
            <div v-if="day.itineraryItems.length > 0">
              <p class="text-xs font-semibold text-gray-500 mb-2">일정</p>
              <div class="space-y-2">
                <div v-for="item in day.itineraryItems" :key="'it-' + item.id" class="flex justify-between items-center text-sm bg-gray-50 rounded-lg px-3 py-2">
                  <div>
                    <p class="font-medium text-gray-800">{{ item.locationName }}</p>
                    <p v-if="item.category" class="text-xs text-gray-500">{{ item.category }}</p>
                  </div>
                  <p class="font-semibold text-gray-700">₩{{ item.expenseAmount.toLocaleString('ko-KR') }}</p>
                </div>
              </div>
            </div>

            <!-- 숙소 비용 -->
            <div v-if="day.accommodations.length > 0">
              <p class="text-xs font-semibold text-gray-500 mb-2">숙소</p>
              <div class="space-y-2">
                <div v-for="item in day.accommodations" :key="'acc-' + item.id" class="flex justify-between items-center text-sm bg-blue-50 rounded-lg px-3 py-2">
                  <div>
                    <p class="font-medium text-gray-800">{{ item.name }}</p>
                    <p class="text-xs text-gray-500">{{ formatDate(item.checkInTime) }} ~ {{ formatDate(item.checkOutTime) }}</p>
                  </div>
                  <p class="font-semibold text-blue-700">₩{{ item.expenseAmount.toLocaleString('ko-KR') }}</p>
                </div>
              </div>
            </div>

            <!-- 교통 비용 -->
            <div v-if="day.transportations.length > 0">
              <p class="text-xs font-semibold text-gray-500 mb-2">교통</p>
              <div class="space-y-2">
                <div v-for="item in day.transportations" :key="'tr-' + item.id" class="flex justify-between items-center text-sm bg-green-50 rounded-lg px-3 py-2">
                  <div>
                    <p class="font-medium text-gray-800">{{ getCategoryIcon(item.category) }} {{ item.category }}</p>
                    <p v-if="item.departureLocation || item.arrivalLocation" class="text-xs text-gray-500">{{ item.departureLocation }} → {{ item.arrivalLocation }}</p>
                  </div>
                  <p class="font-semibold text-green-700">₩{{ getFlightAmount(item).toLocaleString('ko-KR') }}</p>
                </div>
              </div>
            </div>

            <div v-if="day.itineraryItems.length === 0 && day.accommodations.length === 0 && day.transportations.length === 0" class="text-center py-4 text-gray-400">
              지출 내역이 없습니다
            </div>
          </div>
        </div>

        <!-- 여행 전체 비용 카드 -->
        <div v-if="tripWideExpenses.items.length > 0" class="bg-white rounded-2xl shadow-md overflow-hidden border-2 border-primary-200">
          <button @click="showTripWide = !showTripWide" class="w-full p-4 flex justify-between items-center">
            <div>
              <h3 class="text-lg font-bold text-gray-900">여행 전체 비용</h3>
              <p class="text-xs text-gray-500">기간 전체에 걸친 비용</p>
            </div>
            <div class="flex items-center gap-2">
              <p class="text-xl font-bold text-primary-600">₩{{ tripWideExpenses.total.toLocaleString('ko-KR') }}</p>
              <svg class="w-5 h-5 text-gray-400 transition-transform" :class="{ 'rotate-180': showTripWide }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </div>
          </button>
          <div v-if="showTripWide" class="border-t border-gray-100 p-4 space-y-2">
            <div v-for="item in tripWideExpenses.items" :key="item.category" class="flex justify-between items-center text-sm bg-purple-50 rounded-lg px-3 py-2">
              <p class="font-medium text-gray-800">{{ getCategoryIcon(item.category) }} {{ item.category }}</p>
              <p class="font-semibold text-purple-700">₩{{ item.amount.toLocaleString('ko-KR') }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- 카테고리별 뷰 -->
      <div v-if="currentView === 'category'" class="space-y-4">
        <!-- 도넛 차트 -->
        <div class="bg-white rounded-2xl shadow-md p-6">
          <h3 class="font-bold text-gray-900 mb-4 text-center">지출 분포</h3>
          <div class="relative">
            <VueApexCharts
              type="donut"
              height="320"
              :options="chartOptions"
              :series="chartSeries"
            />
          </div>
        </div>

        <!-- 카테고리별 상세 -->
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
            <template v-if="category.key === 'itinerary'">
              <div v-for="item in trip.itineraryItems?.filter(i => i.expenseAmount > 0)" :key="item.id" class="flex justify-between items-start text-sm bg-gray-50 rounded-lg px-3 py-2">
                <div>
                  <p class="font-medium text-gray-900">{{ item.locationName }}</p>
                  <p class="text-xs text-gray-500">Day {{ item.dayNumber }} {{ item.category ? '· ' + item.category : '' }}</p>
                </div>
                <p class="font-semibold text-gray-700">₩{{ item.expenseAmount.toLocaleString('ko-KR') }}</p>
              </div>
            </template>
            <template v-else-if="category.key === 'accommodation'">
              <div v-for="item in trip.accommodations?.filter(a => a.expenseAmount > 0)" :key="item.id" class="flex justify-between items-start text-sm bg-gray-50 rounded-lg px-3 py-2">
                <div>
                  <p class="font-medium text-gray-900">{{ item.name }}</p>
                  <p class="text-xs text-gray-500">{{ formatDate(item.checkInTime) }} ~ {{ formatDate(item.checkOutTime) }}</p>
                </div>
                <p class="font-semibold text-gray-700">₩{{ item.expenseAmount.toLocaleString('ko-KR') }}</p>
              </div>
            </template>
            <template v-else-if="category.key === 'transportation'">
              <div v-for="item in trip.flights?.filter(f => getFlightAmount(f) > 0)" :key="item.id" class="flex justify-between items-start text-sm bg-gray-50 rounded-lg px-3 py-2">
                <div>
                  <p class="font-medium text-gray-900">{{ getCategoryIcon(item.category) }} {{ item.category }}</p>
                  <p v-if="item.departureLocation || item.arrivalLocation" class="text-xs text-gray-500">{{ item.departureLocation }} → {{ item.arrivalLocation }}</p>
                </div>
                <p class="font-semibold text-gray-700">₩{{ getFlightAmount(item).toLocaleString('ko-KR') }}</p>
              </div>
            </template>
            <div v-if="category.count === 0" class="text-center py-4 text-gray-400">
              지출 내역이 없습니다
            </div>
          </div>
        </div>
      </div>
    </div>

    <BottomNavigationBar v-if="tripId || trip.id" :trip-id="tripId || trip.id" :share-token="shareToken" :show="!uiStore.isModalOpen" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import MainHeader from '@/components/common/MainHeader.vue';
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue';
import { useUIStore } from '@/stores/ui';
import apiClient from '@/services/api';
import dayjs from 'dayjs';
import * as XLSX from 'xlsx';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import '@/utils/NanumGothic-Regular-normal.js';
import VueApexCharts from 'vue3-apexcharts';

const props = defineProps({
  shareToken: String,
  readonly: { type: Boolean, default: false }
});

const uiStore = useUIStore();
const route = useRoute();
const router = useRouter();

const tripId = computed(() => route.params.id);
const shareToken = computed(() => props.shareToken || route.params.shareToken);
const isSharedView = computed(() => !!shareToken.value);

const loading = ref(true);
const trip = ref({});
const currentView = ref('daily');
const expandedCategories = ref({ itinerary: false, accommodation: false, transportation: false });
const expandedDays = ref([]);
const showTripWide = ref(false);
const showExportMenu = ref(false);
const chart = ref(null); // ApexCharts 인스턴스를 위한 ref

async function loadTrip() {
  try {
    loading.value = true;
    if (shareToken.value) {
      const response = await apiClient.get(`/personal-trips/public/${shareToken.value}`);
      trip.value = response.data;
    } else {
      const response = await apiClient.get(`/personal-trips/${tripId.value}`);
      trip.value = response.data;
    }
    // 기본으로 첫 번째 일자 펼치기
    if (dailyExpensesDetailed.value.length > 0) {
      expandedDays.value = [dailyExpensesDetailed.value[0].dayNumber];
    }
  } catch (error) {
    console.error('Failed to load trip:', error);
    alert('여행 정보를 불러오는데 실패했습니다.');
    router.push(isSharedView.value ? '/home' : '/trips');
  } finally {
    loading.value = false;
  }
}

// 교통편 총액 계산
function getFlightAmount(flight) {
  if (flight.category === '렌트카' || flight.category === '자가용') {
    return (flight.rentalCost || 0) + (flight.fuelCost || 0) + (flight.tollFee || 0) + (flight.parkingFee || 0);
  }
  return flight.amount || 0;
}

// 총 지출 계산
const totalExpenses = computed(() => {
  let total = 0;
  if (trip.value.itineraryItems) {
    total += trip.value.itineraryItems.reduce((sum, item) => sum + (item.expenseAmount || 0), 0);
  }
  if (trip.value.accommodations) {
    total += trip.value.accommodations.reduce((sum, acc) => sum + (acc.expenseAmount || 0), 0);
  }
  if (trip.value.flights) {
    total += trip.value.flights.reduce((sum, f) => sum + getFlightAmount(f), 0);
  }
  return total;
});

// 일자별 상세 통계
const dailyExpensesDetailed = computed(() => {
  if (!trip.value.itineraryItems && !trip.value.flights) return [];

  const days = new Map();
  const tripStartDate = dayjs(trip.value.startDate);
  
  const allItems = [
    ...(trip.value.itineraryItems || []),
    ...(trip.value.accommodations || []),
    ...(trip.value.flights || []),
  ];

  if (allItems.length === 0) return [];

  const maxDayNumber = allItems.reduce((max, item) => {
    if (item.dayNumber) return Math.max(max, item.dayNumber);
    if (item.checkInTime) {
        const day = dayjs(item.checkInTime).diff(tripStartDate, 'day') + 1;
        return Math.max(max, day);
    }
    return max;
  }, 0);


  for (let i = 1; i <= Math.max(maxDayNumber, 1); i++) {
    days.set(i, {
      dayNumber: i,
      itineraryItems: [],
      accommodations: [],
      transportations: [],
      total: 0,
    });
  }

  // 일정 비용
  trip.value.itineraryItems?.forEach(item => {
    if (item.expenseAmount > 0) {
      const day = days.get(item.dayNumber);
      if (day) {
        day.itineraryItems.push(item);
        day.total += item.expenseAmount;
      }
    }
  });
  
  // 택시 비용 (일정에 연결된 것만)
  trip.value.flights?.filter(f => f.category === '택시' && f.itineraryItemId).forEach(taxi => {
    const itinerary = trip.value.itineraryItems?.find(i => i.id === taxi.itineraryItemId);
    if (itinerary) {
      const day = days.get(itinerary.dayNumber);
      if (day) {
        day.transportations.push(taxi);
        day.total += getFlightAmount(taxi);
      }
    }
  });

  return Array.from(days.values()).filter(day => day.total > 0);
});


// 여행 전체 비용
const tripWideExpenses = computed(() => {
  const items = [];
  const categories = ['항공편', '기차', '버스', '렌트카', '자가용'];

  categories.forEach(category => {
    const categoryFlights = trip.value.flights?.filter(f => f.category === category) || [];
    const total = categoryFlights.reduce((sum, f) => sum + getFlightAmount(f), 0);
    if (total > 0) {
      items.push({ category, amount: total, count: categoryFlights.length });
    }
  });

  // 숙소 비용 추가
  const accommodationTotal = trip.value.accommodations?.reduce((sum, acc) => sum + (acc.expenseAmount || 0), 0) || 0;
  if (accommodationTotal > 0) {
    items.push({
      category: '숙소',
      amount: accommodationTotal,
      count: trip.value.accommodations.filter(a => a.expenseAmount > 0).length
    });
  }

  return { items, total: items.reduce((sum, item) => sum + item.amount, 0) };
});

// 카테고리별 통계
const categoryStats = computed(() => {
  const itineraryItems = trip.value.itineraryItems?.filter(i => i.expenseAmount > 0) || [];
  const accommodationItems = trip.value.accommodations?.filter(a => a.expenseAmount > 0) || [];
  const transportationItems = trip.value.flights?.filter(f => getFlightAmount(f) > 0) || [];

  return [
    {
      name: '일정비용',
      key: 'itinerary',
      total: itineraryItems.reduce((sum, i) => sum + i.expenseAmount, 0),
      count: itineraryItems.length,
      icon: '🍽️',
      color: '#FF6384',
    },
    {
      name: '숙소비용',
      key: 'accommodation',
      total: accommodationItems.reduce((sum, a) => sum + (a.expenseAmount || 0), 0),
      count: accommodationItems.length,
      icon: '🏨',
      color: '#36A2EB',
    },
    {
      name: '교통비용',
      key: 'transportation',
      total: transportationItems.reduce((sum, f) => sum + getFlightAmount(f), 0),
      count: transportationItems.length,
      icon: '🚗',
      color: '#4BC0C0',
    },
  ].sort((a, b) => b.total - a.total);
});

// 도넛 차트 데이터 (ApexCharts)
const chartSeries = computed(() =>
  categoryStats.value.filter(c => c.total > 0).map(c => c.total)
);

const chartOptions = computed(() => ({
  chart: {
    type: 'donut',
    fontFamily: 'inherit',
    toolbar: {
      show: false
    }
  },
  labels: categoryStats.value.filter(c => c.total > 0).map(c => c.name),
  colors: ['#FF6384', '#36A2EB', '#4BC0C0', '#FFCE56', '#9966FF'],
  legend: {
    position: 'bottom',
    fontSize: '14px',
    fontFamily: 'NanumGothic',
    markers: {
      width: 12,
      height: 12,
      radius: 6,
    },
    itemMargin: {
      horizontal: 12,
      vertical: 8,
    },
    formatter: (seriesName, opts) => {
      const total = opts.w.globals.seriesTotals.reduce((a, b) => a + b, 0);
      const val = opts.w.globals.series[opts.seriesIndex];
      const pct = total > 0 ? ((val / total) * 100).toFixed(1) : 0;
      return `${seriesName} (${pct}%)`;
    },
  },
  plotOptions: {
    pie: {
      donut: {
        size: '65%',
        labels: {
          show: true,
          name: {
            show: true,
            fontSize: '16px',
            fontWeight: 600,
            color: '#374151',
          },
          value: {
            show: true,
            fontSize: '24px',
            fontWeight: 700,
            color: '#111827',
            formatter: (val) => `₩${Number(val).toLocaleString('ko-KR')}`,
          },
          total: {
            show: true,
            label: '총 지출',
            fontSize: '14px',
            fontWeight: 500,
            color: '#6B7280',
            formatter: () => `₩${totalExpenses.value.toLocaleString('ko-KR')}`,
          },
        },
      },
    },
  },
  dataLabels: {
    enabled: false,
  },
  tooltip: {
    y: {
      formatter: (val) => `₩${val.toLocaleString('ko-KR')}`,
    },
  },
  stroke: {
    show: true,
    width: 3,
    colors: ['#fff'],
  },
  responsive: [{
    breakpoint: 480,
    options: {
      chart: { height: 280 },
      legend: { position: 'bottom' },
    },
  }],
}));

// 유틸리티 함수
function toggleCategory(category) {
  expandedCategories.value[category] = !expandedCategories.value[category];
}

function toggleDay(dayNumber) {
  const index = expandedDays.value.indexOf(dayNumber);
  if (index > -1) {
    expandedDays.value.splice(index, 1);
  } else {
    expandedDays.value.push(dayNumber);
  }
}

function getCategoryIcon(category) {
  const icons = { '항공편': '✈️', '기차': '🚂', '버스': '🚌', '택시': '🚕', '렌트카': '🚗', '자가용': '🚙', '숙소': '🏨' };
  return icons[category] || '📍';
}

function formatDayDate(dayNumber) {
  if (!trip.value.startDate) return '';
  const date = dayjs(trip.value.startDate).add(dayNumber - 1, 'day');
  const weekdays = ['일', '월', '화', '수', '목', '금', '토'];
  return `${date.format('M/D')}(${weekdays[date.day()]})`;
}

function formatDate(dateStr) {
  if (!dateStr) return '';
  return dayjs(dateStr).format('M/D');
}

// Excel 내보내기
function exportToExcel() {
  showExportMenu.value = false;

  const workbook = XLSX.utils.book_new();

  // 요약 시트
  const summaryData = [
    ['여행명', trip.value.title || ''],
    ['기간', `${trip.value.startDate} ~ ${trip.value.endDate}`],
    ['총 지출', totalExpenses.value],
    [],
    ['카테고리', '금액', '건수'],
    ...categoryStats.value.map(c => [c.name, c.total, c.count]),
  ];
  const summarySheet = XLSX.utils.aoa_to_sheet(summaryData);
  XLSX.utils.book_append_sheet(workbook, summarySheet, '요약');

  // 일정 비용 시트
  const itineraryData = [
    ['일자', '장소명', '카테고리', '금액', '메모'],
    ...(trip.value.itineraryItems?.filter(i => i.expenseAmount > 0).map(i => [
      `Day ${i.dayNumber}`,
      i.locationName,
      i.category || '',
      i.expenseAmount,
      i.notes || '',
    ]) || []),
  ];
  const itinerarySheet = XLSX.utils.aoa_to_sheet(itineraryData);
  XLSX.utils.book_append_sheet(workbook, itinerarySheet, '일정비용');

  // 숙소 비용 시트
  const accommodationData = [
    ['숙소명', '체크인', '체크아웃', '주소', '금액', '메모'],
    ...(trip.value.accommodations?.filter(a => a.expenseAmount > 0).map(a => [
      a.name,
      a.checkInTime ? dayjs(a.checkInTime).format('YYYY-MM-DD HH:mm') : '',
      a.checkOutTime ? dayjs(a.checkOutTime).format('YYYY-MM-DD HH:mm') : '',
      a.address || '',
      a.expenseAmount,
      a.notes || '',
    ]) || []),
  ];
  const accommodationSheet = XLSX.utils.aoa_to_sheet(accommodationData);
  XLSX.utils.book_append_sheet(workbook, accommodationSheet, '숙소비용');

  // 교통 비용 시트
  const transportationData = [
    ['카테고리', '출발지', '도착지', '출발일시', '회사/편명', '예약번호', '금액', '렌트비', '유류비', '톨비', '주차비', '메모'],
    ...(trip.value.flights?.filter(f => getFlightAmount(f) > 0).map(f => [
      f.category,
      f.departureLocation || '',
      f.arrivalLocation || '',
      f.departureTime ? dayjs(f.departureTime).format('YYYY-MM-DD HH:mm') : '',
      f.airline ? `${f.airline} ${f.flightNumber || ''}` : '',
      f.bookingReference || '',
      f.amount || 0,
      f.rentalCost || 0,
      f.fuelCost || 0,
      f.tollFee || 0,
      f.parkingFee || 0,
      f.notes || '',
    ]) || []),
  ];
  const transportationSheet = XLSX.utils.aoa_to_sheet(transportationData);
  XLSX.utils.book_append_sheet(workbook, transportationSheet, '교통비용');

  // 일자별 시트
  const dailyData = [
    ['일자', '날짜', '일정비용', '숙소비용', '교통비용', '합계'],
    ...dailyExpensesDetailed.value.map(d => [
      `Day ${d.dayNumber}`,
      formatDayDate(d.dayNumber),
      d.itineraryItems.reduce((sum, i) => sum + i.expenseAmount, 0),
      d.accommodations.reduce((sum, a) => sum + a.expenseAmount, 0),
      d.transportations.reduce((sum, t) => sum + getFlightAmount(t), 0),
      d.total,
    ]),
  ];
  const dailySheet = XLSX.utils.aoa_to_sheet(dailyData);
  XLSX.utils.book_append_sheet(workbook, dailySheet, '일자별요약');

  XLSX.writeFile(workbook, `${trip.value.title || '여행'}_가계부.xlsx`);
}

// PDF 내보내기
async function exportToPDF() {
  showExportMenu.value = false;

  const doc = new jsPDF();
  doc.setFont('NanumGothic', 'normal');

  const pageWidth = doc.internal.pageSize.getWidth();
  const pageHeight = doc.internal.pageSize.getHeight();
  const margin = 15;
  let y = margin;

  // 1. 제목
  doc.setFontSize(22);
  doc.text(`${trip.value.title || '여행'} 가계부`, pageWidth / 2, y, { align: 'center' });
  y += 8;
  doc.setFontSize(12);
  doc.setTextColor(100);
  doc.text(`${trip.value.startDate} ~ ${trip.value.endDate}`, pageWidth / 2, y, { align: 'center' });
  y += 15;

  // 2. 요약 정보
  doc.setFontSize(16);
  doc.text('여행 경비 요약', margin, y);
  y += 8;
  autoTable(doc, {
    startY: y,
    body: [
      ['총 지출', `${totalExpenses.value.toLocaleString('ko-KR')}원`],
      ['예산', `${(trip.value.budget || 0).toLocaleString('ko-KR')}원`],
      ['남은 예산', `${((trip.value.budget || 0) - totalExpenses.value).toLocaleString('ko-KR')}원`],
    ],
    theme: 'grid',
    styles: { font: 'NanumGothic', fontSize: 11 },
    columnStyles: { 0: { fontStyle: 'bold' } },
  });
  y = doc.lastAutoTable.finalY + 15;

  // 3. 지출 분포 차트
  if (chart.value && totalExpenses.value > 0) {
    doc.setFontSize(16);
    doc.text('지출 분포', margin, y);
    y += 8;
    try {
      const chartImage = await chart.value.dataURI();
      const imgWidth = 120;
      const imgHeight = (chartImage.height * imgWidth) / chartImage.width;
      const xPos = (pageWidth - imgWidth) / 2;
      doc.addImage(chartImage.imgURI, 'PNG', xPos, y, imgWidth, imgHeight);
      y += imgHeight + 15;
    } catch (e) {
      console.error("차트 이미지 생성 실패:", e);
    }
  }
  
  // 4. 카테고리별 지출 상세
  if (y > pageHeight - 40) { y = margin; doc.addPage(); }
  doc.setFontSize(16);
  doc.text('카테고리별 지출 상세', margin, y);
  y += 8;

  const categoryBody = categoryStats.value.map(c => [c.name, `${c.total.toLocaleString()}원`, `${c.count}건`]);
  autoTable(doc, {
    startY: y,
    head: [['카테고리', '총액', '건수']],
    body: categoryBody,
    theme: 'striped',
    styles: { font: 'NanumGothic' },
    headStyles: { fillColor: [63, 162, 225] },
  });
  y = doc.lastAutoTable.finalY + 15;

  // 5. 일자별 지출 상세
  if (dailyExpensesDetailed.value.length > 0) {
      if (y > pageHeight - 40) { y = margin; doc.addPage(); }
      doc.setFontSize(16);
      doc.text('일자별 지출 상세', margin, y);
      y += 8;

      const dailyBody = [];
      dailyExpensesDetailed.value.forEach(day => {
          dailyBody.push([
              { content: `Day ${day.dayNumber} (${formatDayDate(day.dayNumber)})`, colSpan: 3, styles: { fontStyle: 'bold', fillColor: '#f0f0f0' } }
          ]);
          day.itineraryItems.forEach(item => {
              dailyBody.push(['일정', item.locationName, `${item.expenseAmount.toLocaleString()}원`]);
          });
          day.transportations.forEach(item => {
              dailyBody.push(['교통', `${item.category}: ${item.departureLocation || ''} -> ${item.arrivalLocation || ''}`, `${getFlightAmount(item).toLocaleString()}원`]);
          });
      });

      autoTable(doc, {
          startY: y,
          head: [['구분', '내용', '금액']],
          body: dailyBody,
          theme: 'striped',
          styles: { font: 'NanumGothic' },
          headStyles: { fillColor: [63, 162, 225] },
      });
      y = doc.lastAutoTable.finalY + 15;
  }
  
  // 6. 여행 전체 비용 상세
  if (tripWideExpenses.value.items.length > 0) {
      if (y > pageHeight - 40) { y = margin; doc.addPage(); }
      doc.setFontSize(16);
      doc.text('여행 전체 지출 상세 (일자 무관)', margin, y);
      y += 8;

      const tripWideBody = tripWideExpenses.value.items.map(item => [item.category, `${item.count}건`, `${item.amount.toLocaleString()}원`]);
      autoTable(doc, {
          startY: y,
          head: [['구분', '건수', '금액']],
          body: tripWideBody,
          theme: 'striped',
          styles: { font: 'NanumGothic' },
          headStyles: { fillColor: [63, 162, 225] },
      });
  }

  doc.save(`${trip.value.title || '여행'}_가계부_상세.pdf`);
}


onMounted(() => {
  loadTrip();
});
</script>
