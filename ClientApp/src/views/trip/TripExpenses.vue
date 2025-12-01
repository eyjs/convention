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

            <div v-if="day.itineraryItems.length === 0 && day.transportations.length === 0" class="text-center py-4 text-gray-400">
              지출 내역이 없습니다
            </div>
          </div>
        </div>

        <!-- 여행 전체 비용 카드 -->
        <div v-if="tripWideExpenses.items.length > 0" class="bg-white rounded-2xl shadow-md overflow-hidden border-2 border-primary-200">
          <button @click="showTripWide = !showTripWide" class="w-full p-4 flex justify-between items-center">
            <div>
              <h3 class="text-lg font-bold text-gray-900">공통 비용</h3>
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
              ref="chart"
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

    <BottomNavigationBar
      v-if="((tripId && tripId !== 'undefined') || (shareToken && shareToken !== 'undefined')) && !uiStore.isModalOpen"
      :trip-id="tripId || trip.id"
      :share-token="shareToken"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, createApp } from 'vue';
import { useRouter } from 'vue-router';
import MainHeader from '@/components/common/MainHeader.vue';
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue';
import { useUIStore } from '@/stores/ui';
import apiClient from '@/services/api';
import dayjs from 'dayjs';
import * as XLSX from 'xlsx';
import { jsPDF } from 'jspdf';
import html2canvas from 'html2canvas';
import VueApexCharts from 'vue3-apexcharts';
import PrintableExpenses from './PrintableExpenses.vue';


const props = defineProps({
  id: String,               // 라우터에서 자동 주입 (params.id)
  shareToken: String,
  readonly: { type: Boolean, default: false }
});

const uiStore = useUIStore();
const router = useRouter();

const tripId = computed(() => props.id || null);
const shareToken = computed(() => props.shareToken || null);
const isSharedView = computed(() => !!shareToken.value);

const loading = ref(true);
const trip = ref({});
const currentView = ref('daily');
const expandedCategories = ref({ itinerary: false, accommodation: false, transportation: false });
const expandedDays = ref([]);
const showTripWide = ref(false);
const showExportMenu = ref(false);
const chart = ref(null);

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

function getFlightAmount(flight) {
  if (flight.category === '렌트카' || flight.category === '자가용') {
    return (flight.rentalCost || 0) + (flight.fuelCost || 0) + (flight.tollFee || 0) + (flight.parkingFee || 0);
  }
  return flight.amount || 0;
}

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

const dailyExpensesDetailed = computed(() => {
  if (!trip.value.startDate) return [];

  const days = new Map();
  const tripStartDate = dayjs(trip.value.startDate);
  const tripEndDate = dayjs(trip.value.endDate);
  const numDays = tripEndDate.diff(tripStartDate, 'day') + 1;

  for (let i = 1; i <= numDays; i++) {
    days.set(i, {
      dayNumber: i,
      itineraryItems: [],
      transportations: [],
      total: 0,
    });
  }

  trip.value.itineraryItems?.forEach(item => {
    if (item.expenseAmount > 0 && item.dayNumber) {
      const day = days.get(item.dayNumber);
      if (day) {
        day.itineraryItems.push(item);
        day.total += item.expenseAmount;
      }
    }
  });

  trip.value.flights?.filter(f => f.category === '택시' && f.itineraryItemId).forEach(taxi => {
    const itinerary = trip.value.itineraryItems?.find(i => i.id === taxi.itineraryItemId);
    if (itinerary && itinerary.dayNumber) {
      const day = days.get(itinerary.dayNumber);
      if (day) {
        day.transportations.push(taxi);
        day.total += getFlightAmount(taxi);
      }
    }
  });

  return Array.from(days.values()).filter(day => day.total > 0 );
});

const tripWideExpenses = computed(() => {
  const items = [];
  const categories = ['항공편', '기차', '버스', '렌트카', '자가용'];

  categories.forEach(category => {
    const categoryFlights = trip.value.flights?.filter(f => f.category === category && !f.itineraryItemId) || [];
    const total = categoryFlights.reduce((sum, f) => sum + getFlightAmount(f), 0);
    if (total > 0) {
      items.push({ category, amount: total, count: categoryFlights.length });
    }
  });

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

const categoryStats = computed(() => {
  const itineraryItems = trip.value.itineraryItems?.filter(i => i.expenseAmount > 0) || [];
  const accommodationItems = trip.value.accommodations?.filter(a => a.expenseAmount > 0) || [];
  const transportationItems = trip.value.flights?.filter(f => getFlightAmount(f) > 0) || [];

  return [
    { name: '일정비용', key: 'itinerary', total: itineraryItems.reduce((sum, i) => sum + i.expenseAmount, 0), count: itineraryItems.length, icon: '🍽️' },
    { name: '숙소비용', key: 'accommodation', total: accommodationItems.reduce((sum, a) => sum + (a.expenseAmount || 0), 0), count: accommodationItems.length, icon: '🏨' },
    { name: '교통비용', key: 'transportation', total: transportationItems.reduce((sum, f) => sum + getFlightAmount(f), 0), count: transportationItems.length, icon: '🚗' },
  ].sort((a, b) => b.total - a.total);
});

const chartSeries = computed(() => categoryStats.value.filter(c => c.total > 0).map(c => c.total));
const chartOptions = computed(() => ({
    chart: { type: 'donut', fontFamily: 'inherit', toolbar: { show: false } },
    labels: categoryStats.value.filter(c => c.total > 0).map(c => c.name),
    colors: ['#FF6384', '#36A2EB', '#4BC0C0', '#FFCE56', '#9966FF'],
    legend: { position: 'bottom', fontSize: '14px', fontFamily: 'inherit' },
    plotOptions: { pie: { donut: { size: '65%', labels: { show: true, total: { show: true, label: '총 지출', formatter: () => `₩${totalExpenses.value.toLocaleString('ko-KR')}` } } } } },
    dataLabels: { enabled: false },
    tooltip: { y: { formatter: (val) => `₩${val.toLocaleString('ko-KR')}` } },
}));

function toggleCategory(category) { expandedCategories.value[category] = !expandedCategories.value[category]; }
function toggleDay(dayNumber) {
  const index = expandedDays.value.indexOf(dayNumber);
  if (index > -1) expandedDays.value.splice(index, 1);
  else expandedDays.value.push(dayNumber);
}
function getCategoryIcon(category) { return { '항공편': '✈️', '기차': '🚂', '버스': '🚌', '택시': '🚕', '렌트카': '🚗', '자가용': '🚙', '숙소': '🏨' }[category] || '📍'; }
function formatDayDate(dayNumber) {
  if (!trip.value.startDate) return '';
  const date = dayjs(trip.value.startDate).add(dayNumber - 1, 'day');
  return `${date.format('M/D')}(${['일', '월', '화', '수', '목', '금', '토'][date.day()]})`;
}
function formatDate(dateStr) { return dateStr ? dayjs(dateStr).format('M/D') : ''; }

function exportToExcel() {
    showExportMenu.value = false;
    const wb = XLSX.utils.book_new();

    const dailyData = [['일자', '구분', '내용', '카테고리', '금액']];
    const tripStartDate = dayjs(trip.value.startDate);
    const numDays = dayjs(trip.value.endDate).diff(tripStartDate, 'day') + 1;

    for (let i = 1; i <= numDays; i++) {
        const dateStr = tripStartDate.add(i - 1, 'day').format('YYYY-MM-DD');
        let dailyTotal = 0;
        const dayItems = [];

        trip.value.itineraryItems?.filter(item => item.dayNumber === i && item.expenseAmount > 0).forEach(item => {
            dayItems.push([`Day ${i}`, '일정', item.locationName, item.category || '기타', item.expenseAmount]);
            dailyTotal += item.expenseAmount;
        });

        trip.value.flights?.filter(f => f.category === '택시' && f.itineraryItemId).forEach(taxi => {
            const itinerary = trip.value.itineraryItems?.find(item => item.id === taxi.itineraryItemId);
            if (itinerary && itinerary.dayNumber === i) {
                const amount = getFlightAmount(taxi);
                dayItems.push([`Day ${i}`, '교통', `택시: ${taxi.departureLocation || ''} → ${taxi.arrivalLocation || ''}`, '교통', amount]);
                dailyTotal += amount;
            }
        });
        if (dayItems.length > 0) {
            dailyData.push([`${dateStr} (Day ${i})`, '', '', '일계', dailyTotal]);
            dailyData.push(...dayItems);
        }
    }
    
    const tripWideItems = [];
     trip.value.accommodations?.filter(a => a.expenseAmount > 0).forEach(acc => {
        tripWideItems.push(['전체', '숙소', acc.name, acc.type || '숙소', acc.expenseAmount]);
    });
     trip.value.flights?.filter(f => f.category !== '택시' && getFlightAmount(f) > 0).forEach(f => {
        tripWideItems.push(['전체', '교통', `${f.category}: ${f.departureLocation || ''} → ${f.arrivalLocation || ''}`, '교통', getFlightAmount(f)]);
    });
    if (tripWideItems.length > 0) {
        dailyData.push([]);
        dailyData.push(['여행 전체 비용', '', '', '합계', tripWideExpenses.value.total]);
        dailyData.push(...tripWideItems);
    }
    const dailySheet = XLSX.utils.aoa_to_sheet(dailyData);
    XLSX.utils.book_append_sheet(wb, dailySheet, '일자별 상세');

    const categoryData = [['대분류', '소분류', '내용', '금액']];
    categoryStats.value.forEach(cat => {
        if (cat.total > 0) {
            categoryData.push([cat.name, '', '합계', cat.total]);
            if (cat.key === 'itinerary') {
                trip.value.itineraryItems?.filter(i => i.expenseAmount > 0).forEach(item => {
                    categoryData.push(['', item.category || '기타', item.locationName, item.expenseAmount]);
                });
            } else if (cat.key === 'accommodation') {
                trip.value.accommodations?.filter(a => a.expenseAmount > 0).forEach(acc => {
                    categoryData.push(['', acc.type || '숙소', acc.name, acc.expenseAmount]);
                });
            } else if (cat.key === 'transportation') {
                trip.value.flights?.filter(f => getFlightAmount(f) > 0).forEach(flight => {
                    categoryData.push(['', flight.category, `${flight.departureLocation || ''} → ${flight.arrivalLocation || ''}`, getFlightAmount(flight)]);
                });
            }
        }
    });
    const categorySheet = XLSX.utils.aoa_to_sheet(categoryData);
    XLSX.utils.book_append_sheet(wb, categorySheet, '카테고리별 상세');
    
    XLSX.writeFile(wb, `${trip.value.title || '여행'}_가계부_상세.xlsx`);
}

async function exportToPDF() {
  showExportMenu.value = false;

  const printContainer = document.createElement('div');
  printContainer.style.position = 'fixed';
  printContainer.style.left = '-9999px';
  document.body.appendChild(printContainer);
  
  const printApp = createApp(PrintableExpenses, {
    trip: trip.value,
  });
  
  printApp.mount(printContainer);

  // Wait for component to render
  await new Promise(resolve => setTimeout(resolve, 500));

  try {
    const canvas = await html2canvas(printContainer.firstElementChild, {
        scale: 2,
        useCORS: true,
        allowTaint: true,
        backgroundColor: '#ffffff'
    });

    const imgData = canvas.toDataURL('image/png');
    const pdf = new jsPDF({
        orientation: 'p',
        unit: 'mm',
        format: 'a4',
    });

    const pdfWidth = pdf.internal.pageSize.getWidth();
    const imgWidth = canvas.width;
    const imgHeight = canvas.height;
    const ratio = imgWidth / pdfWidth;
    const scaledHeight = imgHeight / ratio;

    let heightLeft = scaledHeight;
    let position = 0;

    pdf.addImage(imgData, 'PNG', 0, position, pdfWidth, scaledHeight);
    heightLeft -= pdf.internal.pageSize.getHeight();

    while (heightLeft > 0) {
        position -= pdf.internal.pageSize.getHeight();
        pdf.addPage();
        pdf.addImage(imgData, 'PNG', 0, position, pdfWidth, scaledHeight);
        heightLeft -= pdf.internal.pageSize.getHeight();
    }
    
    pdf.save(`${trip.value.title || '여행'}_지출_레포트.pdf`);

  } catch (e) {
    console.error("PDF 생성에 실패했습니다.", e);
    alert("PDF 생성 중 오류가 발생했습니다.");
  } finally {
    printApp.unmount();
    document.body.removeChild(printContainer);
  }
}

onMounted(() => {
  loadTrip();
});

// Watch for route changes (when navigating between different trips)
watch(() => props.id, async (newId, oldId) => {
  if (newId && newId !== oldId) {
    await loadTrip();
  }
});
</script>