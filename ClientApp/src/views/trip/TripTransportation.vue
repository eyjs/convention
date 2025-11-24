<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader :title="'교통편'" :show-back="true" />

    <div v-if="loading" class="text-center py-20">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
      <p class="mt-4 text-gray-600 font-medium">교통편을 불러오는 중...</p>
    </div>

    <div v-else class="max-w-2xl mx-auto px-4 py-4 pb-24">
      <!-- 카테고리별 섹션 -->
      <div class="space-y-4">
        <div v-for="category in categories" :key="category.name" class="bg-white rounded-2xl shadow-md overflow-hidden">
          <!-- 카테고리 헤더 -->
          <button
            @click="toggleCategory(category.name)"
            class="w-full p-4 flex justify-between items-center"
          >
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-full flex items-center justify-center" :class="category.bgClass">
                <component :is="category.icon" class="w-5 h-5" :class="category.iconClass" />
              </div>
              <div class="text-left">
                <h3 class="font-bold text-gray-900">{{ category.name }}</h3>
                <p class="text-xs text-gray-500">{{ getFlightsByCategory(category.name).length }}건</p>
              </div>
            </div>
            <svg
              class="w-5 h-5 text-gray-400 transition-transform"
              :class="{ 'rotate-180': expandedCategories.includes(category.name) }"
              fill="none" stroke="currentColor" viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
            </svg>
          </button>

          <!-- 카테고리 내용 -->
          <div v-if="expandedCategories.includes(category.name)" class="border-t border-gray-100">
            <div class="p-4 space-y-3">
              <!-- 교통편 목록 -->
              <div v-if="getFlightsByCategory(category.name).length > 0">
                <div
                  v-for="flight in getFlightsByCategory(category.name)"
                  :key="flight.id"
                  @click="!effectiveReadonly && openDetailModal(flight)"
                  class="border rounded-xl p-4 bg-gray-50 hover:bg-gray-100 transition-colors"
                  :class="{ 'cursor-pointer': !effectiveReadonly }"
                >
                  <div class="flex justify-between items-start">
                    <div class="flex-1 min-w-0">
                      <!-- 항공편 -->
                      <template v-if="flight.category === '항공편'">
                        <div class="flex items-center gap-2 mb-1">
                          <span v-if="flight.airline" class="font-semibold text-gray-900">{{ flight.airline }}</span>
                          <span v-if="flight.flightNumber" class="text-sm text-gray-600">{{ flight.flightNumber }}</span>
                        </div>
                        <div v-if="flight.departureLocation || flight.arrivalLocation" class="flex items-center gap-2 text-sm text-gray-600 mb-1">
                          <span>{{ flight.departureLocation || '출발지' }}</span>
                          <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                          </svg>
                          <span>{{ flight.arrivalLocation || '도착지' }}</span>
                        </div>
                        <div v-if="flight.departureTime" class="text-xs text-gray-500">
                          {{ formatDateTime(flight.departureTime) }}
                          <span v-if="flight.arrivalTime"> ~ {{ formatDateTime(flight.arrivalTime) }}</span>
                        </div>
                        <div v-if="flight.bookingReference" class="text-xs text-gray-500 mt-1">
                          예약번호: {{ flight.bookingReference }}
                        </div>
                      </template>

                      <!-- 기차 (간소화) -->
                      <template v-else-if="flight.category === '기차'">
                        <div v-if="flight.departureLocation || flight.arrivalLocation" class="flex items-center gap-2 text-sm text-gray-700 mb-1">
                          <span>{{ flight.departureLocation || '출발역' }}</span>
                          <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                          </svg>
                          <span>{{ flight.arrivalLocation || '도착역' }}</span>
                        </div>
                        <div v-if="flight.departureTime" class="text-xs text-gray-500">
                          {{ formatDateTime(flight.departureTime) }}
                        </div>
                      </template>

                      <!-- 버스 (간소화) -->
                      <template v-else-if="flight.category === '버스'">
                        <div v-if="flight.departureLocation || flight.arrivalLocation" class="flex items-center gap-2 text-sm text-gray-700 mb-1">
                          <span>{{ flight.departureLocation || '출발지' }}</span>
                          <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                          </svg>
                          <span>{{ flight.arrivalLocation || '도착지' }}</span>
                        </div>
                        <div v-if="flight.departureTime" class="text-xs text-gray-500">
                          {{ formatDate(flight.departureTime) }}
                        </div>
                      </template>

                      <!-- 택시 -->
                      <template v-else-if="flight.category === '택시'">
                        <div v-if="flight.departureLocation || flight.arrivalLocation" class="flex items-center gap-2 text-sm text-gray-700 mb-1">
                          <span>{{ flight.departureLocation || '출발지' }}</span>
                          <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                          </svg>
                          <span>{{ flight.arrivalLocation || '도착지' }}</span>
                        </div>
                        <div v-if="getLinkedItinerary(flight.itineraryItemId)" class="text-xs text-gray-500">
                          연결된 일정: {{ getLinkedItinerary(flight.itineraryItemId)?.locationName }}
                        </div>
                      </template>

                      <!-- 렌트카/자가용 -->
                      <template v-else-if="['렌트카', '자가용'].includes(flight.category)">
                        <div v-if="flight.airline" class="font-semibold text-gray-900 mb-1">{{ flight.airline }}</div>
                        <div v-if="flight.departureTime" class="text-xs text-gray-500">
                          {{ formatDate(flight.departureTime) }}
                          <span v-if="flight.arrivalTime"> ~ {{ formatDate(flight.arrivalTime) }}</span>
                        </div>
                      </template>

                      <!-- 메모 -->
                      <p v-if="flight.notes" class="text-xs text-gray-500 mt-2 italic">{{ flight.notes }}</p>
                    </div>
                  </div>
                </div>
              </div>
              <div v-else class="text-center py-6 text-gray-400">
                등록된 {{ category.name }} 없습니다
              </div>

              <!-- 추가 버튼 -->
              <button
                v-if="!effectiveReadonly"
                @click="openAddModal(category.name)"
                class="w-full py-3 border-2 border-dashed rounded-xl font-medium transition-all"
                :class="category.addButtonClass"
              >
                + {{ category.name }} 추가
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 교통편 추가/수정 모달 -->
    <SlideUpModal :is-open="isEditModalOpen" @close="closeEditModal" z-index-class="z-[60]">
      <template #header-title>{{ editingFlight?.id ? '교통편 수정' : '교통편 추가' }}</template>
      <template #body>
        <form id="transportation-form" @submit.prevent="saveTransportation" class="space-y-4">
          <!-- 카테고리 (읽기전용) -->
          <div>
            <label class="label">카테고리</label>
            <input v-model="flightData.category" type="text" class="input bg-gray-100" readonly />
          </div>

          <!-- 항공편 전용 필드 -->
          <template v-if="flightData.category === '항공편'">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">항공사</label>
                <input v-model="flightData.airline" type="text" class="input" placeholder="대한항공, 아시아나 등" />
              </div>
              <div>
                <label class="label">편명</label>
                <input v-model="flightData.flightNumber" type="text" class="input" placeholder="KE123" />
              </div>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">출발지</label>
                <input v-model="flightData.departureLocation" type="text" class="input" placeholder="출발 공항" />
              </div>
              <div>
                <label class="label">도착지</label>
                <input v-model="flightData.arrivalLocation" type="text" class="input" placeholder="도착 공항" />
              </div>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">출발 일시</label>
                <input v-model="flightData.departureTime" type="datetime-local" class="input" />
              </div>
              <div>
                <label class="label">도착 일시</label>
                <input v-model="flightData.arrivalTime" type="datetime-local" class="input" />
              </div>
            </div>
            <div>
              <label class="label">예약번호</label>
              <input v-model="flightData.bookingReference" type="text" class="input" placeholder="예약번호 (선택)" />
            </div>
            <div>
              <label class="label">금액 (원)</label>
              <input v-model.number="flightData.amount" type="number" class="input" placeholder="예: 150000" min="0" step="100" />
            </div>
          </template>

          <!-- 기차 전용 필드 (간소화) -->
          <template v-else-if="flightData.category === '기차'">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">출발역</label>
                <input v-model="flightData.departureLocation" type="text" class="input" placeholder="서울역" />
              </div>
              <div>
                <label class="label">도착역</label>
                <input v-model="flightData.arrivalLocation" type="text" class="input" placeholder="부산역" />
              </div>
            </div>
            <div>
              <label class="label">출발 일시</label>
              <input v-model="flightData.departureTime" type="datetime-local" class="input" />
            </div>
            <div>
              <label class="label">금액 (원)</label>
              <input v-model.number="flightData.amount" type="number" class="input" placeholder="예: 50000" min="0" step="100" />
            </div>
          </template>

          <!-- 버스 전용 필드 (간소화) -->
          <template v-else-if="flightData.category === '버스'">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">출발지</label>
                <input v-model="flightData.departureLocation" type="text" class="input" placeholder="출발 터미널" />
              </div>
              <div>
                <label class="label">도착지</label>
                <input v-model="flightData.arrivalLocation" type="text" class="input" placeholder="도착 터미널" />
              </div>
            </div>
            <div>
              <label class="label">출발 날짜</label>
              <input v-model="flightData.departureDate" type="date" class="input" />
            </div>
            <div>
              <label class="label">금액 (원)</label>
              <input v-model.number="flightData.amount" type="number" class="input" placeholder="예: 30000" min="0" step="100" />
            </div>
          </template>

          <!-- 택시 전용 필드 -->
          <template v-else-if="flightData.category === '택시'">
            <div>
              <label class="label">연결할 일정 (선택)</label>
              <select v-model="flightData.itineraryItemId" class="input">
                <option :value="null">선택 안함</option>
                <option v-for="item in trip.itineraryItems" :key="item.id" :value="item.id">
                  Day {{ item.dayNumber }} - {{ item.locationName }}
                </option>
              </select>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">출발지</label>
                <input v-model="flightData.departureLocation" type="text" class="input" placeholder="출발지" />
              </div>
              <div>
                <label class="label">도착지</label>
                <input v-model="flightData.arrivalLocation" type="text" class="input" placeholder="도착지" />
              </div>
            </div>
            <div>
              <label class="label">금액 (원)</label>
              <input v-model.number="flightData.amount" type="number" class="input" placeholder="예: 15000" min="0" step="100" />
            </div>
          </template>

          <!-- 렌트카 전용 필드 -->
          <template v-else-if="flightData.category === '렌트카'">
            <div>
              <label class="label">렌트회사</label>
              <input v-model="flightData.airline" type="text" class="input" placeholder="렌트회사명" />
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">대여 일시</label>
                <input v-model="flightData.departureTime" type="datetime-local" class="input" />
              </div>
              <div>
                <label class="label">반납 일시</label>
                <input v-model="flightData.arrivalTime" type="datetime-local" class="input" />
              </div>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">렌트비 (원)</label>
                <input v-model.number="flightData.rentalCost" type="number" class="input" placeholder="예: 100000" min="0" step="100" />
              </div>
              <div>
                <label class="label">유류비 (원)</label>
                <input v-model.number="flightData.fuelCost" type="number" class="input" placeholder="예: 50000" min="0" step="100" />
              </div>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">톨비 (원)</label>
                <input v-model.number="flightData.tollFee" type="number" class="input" placeholder="예: 20000" min="0" step="100" />
              </div>
              <div>
                <label class="label">주차비 (원)</label>
                <input v-model.number="flightData.parkingFee" type="number" class="input" placeholder="예: 15000" min="0" step="100" />
              </div>
            </div>
          </template>

          <!-- 자가용 전용 필드 -->
          <template v-else-if="flightData.category === '자가용'">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">유류비 (원)</label>
                <input v-model.number="flightData.fuelCost" type="number" class="input" placeholder="예: 50000" min="0" step="100" />
              </div>
              <div>
                <label class="label">톨비 (원)</label>
                <input v-model.number="flightData.tollFee" type="number" class="input" placeholder="예: 20000" min="0" step="100" />
              </div>
            </div>
            <div>
              <label class="label">주차비 (원)</label>
              <input v-model.number="flightData.parkingFee" type="number" class="input" placeholder="예: 15000" min="0" step="100" />
            </div>
          </template>

          <!-- 공통: 메모 -->
          <div>
            <label class="label">메모</label>
            <textarea v-model="flightData.notes" rows="2" class="input" placeholder="메모"></textarea>
          </div>
        </form>
      </template>
      <template #footer>
        <div class="flex gap-3 w-full">
          <button v-if="editingFlight?.id" type="button" @click="deleteTransportation" class="py-3 px-4 bg-red-100 text-red-700 rounded-xl font-semibold hover:bg-red-200 transition-colors">
            삭제
          </button>
          <button type="button" @click="closeEditModal" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 transition-colors">
            취소
          </button>
          <button type="submit" form="transportation-form" class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 transition-all">
            저장
          </button>
        </div>
      </template>
    </SlideUpModal>

    <!-- Bottom Navigation Bar -->
    <BottomNavigationBar v-if="tripId || trip.id" :trip-id="tripId || trip.id" :share-token="shareToken" :show="!uiStore.isModalOpen" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import MainHeader from '@/components/common/MainHeader.vue';
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue';
import SlideUpModal from '@/components/common/SlideUpModal.vue';
import { useUIStore } from '@/stores/ui';
import apiClient from '@/services/api';
import dayjs from 'dayjs';
import { Plane, Train, Bus, Car } from 'lucide-vue-next';

// Props for readonly mode and shared access
const props = defineProps({
  shareToken: String,
  readonly: {
    type: Boolean,
    default: false
  }
})

const uiStore = useUIStore();
const route = useRoute();
const router = useRouter();

// Determine tripId and readonly mode
const tripId = computed(() => route.params.id);
const shareToken = computed(() => props.shareToken || route.params.shareToken);
const isSharedView = computed(() => !!shareToken.value);
const effectiveReadonly = computed(() => props.readonly || isSharedView.value);

const loading = ref(true);
const trip = ref({});
const expandedCategories = ref(['항공편']); // 기본으로 항공편 펼침

// Modal states
const isEditModalOpen = ref(false);
const editingFlight = ref(null);
const flightData = ref({});

// 카테고리 정의
const categories = [
  {
    name: '항공편',
    icon: Plane,
    iconClass: 'text-blue-600',
    bgClass: 'bg-blue-100',
    addButtonClass: 'border-blue-300 text-blue-600 bg-blue-50 hover:bg-blue-100'
  },
  {
    name: '기차',
    icon: Train,
    iconClass: 'text-green-600',
    bgClass: 'bg-green-100',
    addButtonClass: 'border-green-300 text-green-600 bg-green-50 hover:bg-green-100'
  },
  {
    name: '버스',
    icon: Bus,
    iconClass: 'text-orange-600',
    bgClass: 'bg-orange-100',
    addButtonClass: 'border-orange-300 text-orange-600 bg-orange-50 hover:bg-orange-100'
  },
  {
    name: '택시',
    icon: Car,
    iconClass: 'text-purple-600',
    bgClass: 'bg-purple-100',
    addButtonClass: 'border-purple-300 text-purple-600 bg-purple-50 hover:bg-purple-100'
  },
  {
    name: '렌트카',
    icon: Car,
    iconClass: 'text-pink-600',
    bgClass: 'bg-pink-100',
    addButtonClass: 'border-pink-300 text-pink-600 bg-pink-50 hover:bg-pink-100'
  },
  {
    name: '자가용',
    icon: Car,
    iconClass: 'text-indigo-600',
    bgClass: 'bg-indigo-100',
    addButtonClass: 'border-indigo-300 text-indigo-600 bg-indigo-50 hover:bg-indigo-100'
  },
];

// 데이터 로드
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
  } catch (error) {
    console.error('Failed to load trip:', error);
    alert('여행 정보를 불러오는데 실패했습니다.');
    if (isSharedView.value) {
      router.push('/home');
    } else {
      router.push('/trips');
    }
  } finally {
    loading.value = false;
  }
}

// 계산된 값들
const totalTransportationCost = computed(() => {
  if (!trip.value.flights) return 0;
  return trip.value.flights.reduce((sum, f) => sum + getFlightTotalCost(f), 0);
});

function getFlightsByCategory(category) {
  if (!trip.value.flights) return [];
  return trip.value.flights.filter(f => f.category === category);
}

function getCategoryTotal(category) {
  return getFlightsByCategory(category).reduce((sum, f) => sum + getFlightTotalCost(f), 0);
}

function getFlightTotalCost(flight) {
  if (['렌트카', '자가용'].includes(flight.category)) {
    return (flight.rentalCost || 0) + (flight.fuelCost || 0) + (flight.tollFee || 0) + (flight.parkingFee || 0);
  }
  return flight.amount || 0;
}

function getLinkedItinerary(itineraryItemId) {
  if (!itineraryItemId || !trip.value.itineraryItems) return null;
  return trip.value.itineraryItems.find(i => i.id === itineraryItemId);
}

// 카테고리 토글
function toggleCategory(category) {
  const index = expandedCategories.value.indexOf(category);
  if (index > -1) {
    expandedCategories.value.splice(index, 1);
  } else {
    expandedCategories.value.push(category);
  }
}

// 모달 관련
function openAddModal(category) {
  editingFlight.value = null;
  flightData.value = {
    category,
    itineraryItemId: null,
    airline: '',
    flightNumber: '',
    departureLocation: '',
    arrivalLocation: '',
    departureTime: '',
    arrivalTime: '',
    departureDate: '', // 버스용 날짜
    bookingReference: '',
    seatNumber: '',
    amount: null,
    rentalCost: null,
    fuelCost: null,
    tollFee: null,
    parkingFee: null,
    notes: ''
  };
  isEditModalOpen.value = true;
}

function openDetailModal(flight) {
  editingFlight.value = flight;
  flightData.value = {
    ...flight,
    departureTime: flight.departureTime ? dayjs(flight.departureTime).format('YYYY-MM-DDTHH:mm') : '',
    arrivalTime: flight.arrivalTime ? dayjs(flight.arrivalTime).format('YYYY-MM-DDTHH:mm') : '',
    departureDate: flight.departureTime ? dayjs(flight.departureTime).format('YYYY-MM-DD') : '' // 버스용 날짜
  };
  isEditModalOpen.value = true;
}

function closeEditModal() {
  isEditModalOpen.value = false;
  editingFlight.value = null;
}

async function saveTransportation() {
  try {
    // 렌트카/자가용은 합계를 amount에 저장
    if (['렌트카', '자가용'].includes(flightData.value.category)) {
      flightData.value.amount = getFlightTotalCost(flightData.value);
    }

    // 택시가 아니면 일정 연결 제거
    if (flightData.value.category !== '택시') {
      flightData.value.itineraryItemId = null;
    }

    // 버스는 날짜만 사용 -> departureTime으로 변환
    if (flightData.value.category === '버스' && flightData.value.departureDate) {
      flightData.value.departureTime = flightData.value.departureDate + 'T00:00';
    }

    // 페이로드 생성 및 데이터 정리
    const payload = {
      category: flightData.value.category,
      itineraryItemId: flightData.value.itineraryItemId || null,
      amount: flightData.value.amount || null,
      tollFee: flightData.value.tollFee || null,
      fuelCost: flightData.value.fuelCost || null,
      parkingFee: flightData.value.parkingFee || null,
      rentalCost: flightData.value.rentalCost || null,
      airline: flightData.value.airline || null,
      flightNumber: flightData.value.flightNumber || null,
      departureLocation: flightData.value.departureLocation || null,
      arrivalLocation: flightData.value.arrivalLocation || null,
      departureTime: flightData.value.departureTime || null,
      arrivalTime: flightData.value.arrivalTime || null,
      bookingReference: flightData.value.bookingReference || null,
      seatNumber: flightData.value.seatNumber || null,
      notes: flightData.value.notes || null,
    };

    if (editingFlight.value?.id) {
      await apiClient.put(`/personal-trips/flights/${editingFlight.value.id}`, payload);
    } else {
      await apiClient.post(`/personal-trips/${tripId.value}/flights`, payload);
    }

    await loadTrip();
    closeEditModal();
  } catch (error) {
    console.error('Failed to save transportation:', error);
    alert('저장에 실패했습니다.');
  }
}

async function deleteTransportation() {
  if (!confirm('이 교통편을 삭제하시겠습니까?')) return;
  try {
    await apiClient.delete(`/personal-trips/flights/${editingFlight.value.id}`);
    await loadTrip();
    closeEditModal();
  } catch (error) {
    console.error('Failed to delete transportation:', error);
    alert('삭제에 실패했습니다.');
  }
}


// 날짜 포맷팅
function formatDateTime(dateTime) {
  if (!dateTime) return '';
  return dayjs(dateTime).format('YYYY-MM-DD HH:mm');
}

function formatDate(date) {
  if (!date) return '';
  return dayjs(date).format('YYYY-MM-DD');
}

onMounted(() => {
  loadTrip();
});
</script>

<style scoped>
.label {
  @apply block text-sm font-medium text-gray-700 mb-1;
}

.input {
  @apply w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500;
}
</style>
