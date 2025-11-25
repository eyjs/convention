<template>
  <SlideUpModal :is-open="show" @close="emit('close')" z-index-class="z-[60]">
    <template #header-title>항공편 추가</template>
    <template #body>
      <div class="space-y-4">
        <!-- 출발 항공편 등록 -->
        <div class="border-2 rounded-xl p-4" :class="roundTrip.departure ? 'border-blue-300 bg-blue-50' : 'border-gray-200'">
          <div class="flex justify-between items-center mb-3">
            <h3 class="font-bold text-gray-900 flex items-center gap-2">
              <svg class="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8"></path></svg>
              출발 항공편
            </h3>
            <span v-if="roundTrip.departure" class="text-xs px-2 py-1 bg-blue-100 text-blue-700 rounded-full font-medium">✓ 선택됨</span>
          </div>

          <div v-if="!roundTrip.departure">
            <button @click="openFlightSearch('departure')" class="w-full py-3 px-4 bg-blue-500 text-white rounded-xl font-semibold hover:bg-blue-600 transition-colors flex items-center justify-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>
              출발편 검색
            </button>
          </div>

          <div v-else class="space-y-3">
            <FlightInfoDisplay :flight="roundTrip.departure" type="departure" @clear="roundTrip.departure = null" />
            <div>
              <label class="label text-sm text-gray-600">도착 시간 (직접 입력)</label>
              <input v-model="roundTrip.departure.arrivalTime" type="datetime-local" class="input" />
            </div>
          </div>
        </div>

        <!-- 도착 항공편 등록 -->
        <div class="border-2 rounded-xl p-4" :class="roundTrip.arrival ? 'border-green-300 bg-green-50' : 'border-gray-200'">
          <div class="flex justify-between items-center mb-3">
            <h3 class="font-bold text-gray-900 flex items-center gap-2">
              <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H7a3 3 0 00-3 3v8a3 3 0 003 3z"></path></svg>
              도착 항공편
            </h3>
            <span v-if="roundTrip.arrival" class="text-xs px-2 py-1 bg-green-100 text-green-700 rounded-full font-medium">✓ 선택됨</span>
          </div>
          
          <div v-if="!roundTrip.arrival">
            <button @click="openFlightSearch('arrival')" class="w-full py-3 px-4 bg-green-500 text-white rounded-xl font-semibold hover:bg-green-600 transition-colors flex items-center justify-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>
              도착편 검색
            </button>
          </div>

          <div v-else class="space-y-3">
            <FlightInfoDisplay :flight="roundTrip.arrival" type="arrival" @clear="roundTrip.arrival = null" />
            <div>
              <label class="label text-sm text-gray-600">출발 시간 (직접 입력)</label>
              <input v-model="roundTrip.arrival.departureTime" type="datetime-local" class="input" />
            </div>
          </div>
        </div>

        <!-- 비용 및 메모 -->
        <div v-if="roundTrip.departure || roundTrip.arrival" class="border-2 border-gray-200 rounded-xl p-4">
          <h3 class="font-bold text-gray-900 mb-3">추가 정보</h3>
          <div class="space-y-3">
            <div>
              <label class="label">왕복 총 금액 (원)</label>
              <input v-model.number="roundTrip.totalCost" v-number-format type="text" inputmode="numeric" class="input" placeholder="예: 300000" />
            </div>
            <div>
              <label class="label">메모</label>
              <textarea v-model="roundTrip.notes" rows="2" class="input" placeholder="예약번호, 좌석번호 등"></textarea>
            </div>
          </div>
        </div>

      </div>
    </template>

    <template #footer>
      <div class="flex gap-3 w-full">
        <button @click="emit('close')" class="btn-secondary">취소</button>
        <button @click="saveFlights" :disabled="!roundTrip.departure && !roundTrip.arrival" class="btn-primary">저장</button>
      </div>
    </template>
  </SlideUpModal>

  <FlightSearchModal :show="showFlightSearchModal" :flight-type="currentSearchType" @close="showFlightSearchModal = false" @apply="handleFlightSelect" />
</template>

<script setup>
import { ref, reactive, watch } from 'vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import FlightSearchModal from '@/components/trip/FlightSearchModal.vue'
import FlightInfoDisplay from '@/components/personalTrip/FlightInfoDisplay.vue'
import dayjs from 'dayjs'

const props = defineProps({
  show: Boolean,
  existingFlights: { type: Array, default: () => [] }
})
const emit = defineEmits(['close', 'save'])

const roundTrip = reactive({
  departure: null,
  arrival: null,
  totalCost: null,
  notes: ''
});

const showFlightSearchModal = ref(false)
const currentSearchType = ref('') // 'departure' or 'arrival'

watch(() => props.show, (newVal) => {
  if (newVal) {
    resetForm();
    if (props.existingFlights && props.existingFlights.length > 0) {
      // Logic to populate from existing flights if needed (for edit mode)
      const dep = props.existingFlights.find(f => f.departureLocation?.includes('인천') || f.departureAirportCode === 'ICN');
      const arr = props.existingFlights.find(f => f.arrivalLocation?.includes('인천') || f.arrivalAirportCode === 'ICN');
      if(dep) roundTrip.departure = {...dep};
      if(arr) roundTrip.arrival = {...arr};
      // Simple cost/notes logic for now
      roundTrip.totalCost = (dep?.amount || 0) + (arr?.amount || 0);
      roundTrip.notes = dep?.notes || arr?.notes || '';
    }
  }
})

function resetForm() {
    roundTrip.departure = null;
    roundTrip.arrival = null;
    roundTrip.totalCost = null;
    roundTrip.notes = '';
}

function openFlightSearch(type) {
  currentSearchType.value = type
  showFlightSearchModal.value = true
}

function handleFlightSelect(flightInfo) {
  console.log('Selected flight info:', flightInfo);
  const commonData = {
    airline: flightInfo.airline,
    flightNumber: flightInfo.flightId,
    terminal: flightInfo.terminal,
    gate: flightInfo.gate,
    status: flightInfo.status,
  };
  
  if (currentSearchType.value === 'departure') {
    roundTrip.departure = {
      ...commonData,
      departureLocation: '인천국제공항',
      arrivalLocation: flightInfo.airport,
      departureTime: dayjs(`${formatDate(flightInfo.scheduleDate)} ${flightInfo.estimatedTime || flightInfo.scheduleTime}`).format('YYYY-MM-DDTHH:mm'),
      arrivalTime: null, // User input
      departureAirportCode: 'ICN',
      arrivalAirportCode: flightInfo.airportCode,
    };
  } else {
    roundTrip.arrival = {
      ...commonData,
      departureLocation: flightInfo.airport,
      arrivalLocation: '인천국제공항',
      departureTime: null, // User input
      arrivalTime: dayjs(`${formatDate(flightInfo.scheduleDate)} ${flightInfo.estimatedTime || flightInfo.scheduleTime}`).format('YYYY-MM-DDTHH:mm'),
      departureAirportCode: flightInfo.airportCode,
      arrivalAirportCode: 'ICN',
    };
  }
  showFlightSearchModal.value = false;
}

function saveFlights() {
  const flightsToSave = [];
  
  if (roundTrip.departure) {
    flightsToSave.push({
      ...roundTrip.departure,
      category: '항공편',
      amount: roundTrip.totalCost / (roundTrip.arrival ? 2 : 1), // Split cost
      notes: roundTrip.notes
    });
  }
  
  if (roundTrip.arrival) {
    flightsToSave.push({
      ...roundTrip.arrival,
      category: '항공편',
      amount: roundTrip.totalCost / (roundTrip.departure ? 2 : 1), // Split cost
      notes: roundTrip.notes
    });
  }
  
  emit('save', flightsToSave);
}

function formatDate(dateStr) {
  if (!dateStr || dateStr.length !== 8) return dateStr;
  return `${dateStr.substring(0, 4)}-${dateStr.substring(4, 6)}-${dateStr.substring(6, 8)}`;
}

</script>

<style scoped>
.label { @apply block text-sm font-medium text-gray-700 mb-1; }
.input { @apply block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm; }
.btn-primary { @apply flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed; }
.btn-secondary { @apply flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors; }
</style>