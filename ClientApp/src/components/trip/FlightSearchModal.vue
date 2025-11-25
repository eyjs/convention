<template>
  <SlideUpModal :is-open="show" @close="emit('close')" z-index-class="z-[70]">
    <template #header-title>항공편 검색 ({{ flightType === 'departure' ? '출발' : '도착' }})</template>
    <template #body>
      <div class="space-y-4">
        <!-- 검색 폼 -->
        <div class="space-y-3">
          <div>
            <label class="label">날짜 *</label>
            <input
              v-model="searchForm.date"
              type="date"
              class="input"
              @keyup.enter="searchFlight"
            />
          </div>
          <div>
            <label class="label">항공편 번호 *</label>
            <input
              v-model="searchForm.flightNumber"
              type="text"
              class="input"
              placeholder="예: KE123, OZ6889"
              @keyup.enter="searchFlight"
            />
          </div>
        </div>

        <!-- 검색 버튼 -->
        <div class="flex gap-2">
          <button
            @click="searchFlight"
            :disabled="isLoading || !searchForm.date || !searchForm.flightNumber"
            class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors flex items-center justify-center gap-2"
          >
            <svg v-if="isLoading" class="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            {{ isLoading ? '조회 중...' : '항공편 조회' }}
          </button>
        </div>

        <!-- 에러 메시지 -->
        <div v-if="errorMessage" class="bg-red-50 border border-red-200 rounded-lg p-3">
          <p class="text-sm text-red-700">{{ errorMessage }}</p>
        </div>

        <!-- 검색 결과 -->
        <div v-if="flightResult" class="space-y-3">
          <h3 class="font-bold text-gray-900">검색 결과</h3>
          <div
            @click="selectFlight(flightResult)"
            class="border rounded-xl p-4 bg-gray-50 hover:bg-primary-50 hover:border-primary-500 cursor-pointer transition-all"
          >
            <div class="flex justify-between items-start">
              <div class="flex-1">
                <div class="flex items-center gap-2 mb-2">
                  <span class="font-semibold text-gray-900">{{ flightResult.airline }}</span>
                  <span class="text-sm text-gray-600">{{ flightResult.flightId }}</span>
                  <span
                    class="px-2 py-0.5 text-xs font-medium rounded-full"
                    :class="flightResult.type === 'DEPARTURE' ? 'bg-blue-100 text-blue-700' : 'bg-green-100 text-green-700'"
                  >
                    {{ flightResult.type === 'DEPARTURE' ? '출발' : '도착' }}
                  </span>
                </div>
                <div class="flex items-center gap-2 text-sm text-gray-600 mb-1">
                  <span v-if="flightResult.type === 'DEPARTURE'">인천국제공항</span>
                  <span v-else>{{ flightResult.airport }} ({{ flightResult.airportCode }})</span>
                  <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                  </svg>
                  <span v-if="flightResult.type === 'ARRIVAL'">인천국제공항</span>
                  <span v-else>{{ flightResult.airport }} ({{ flightResult.airportCode }})</span>
                </div>
                <div class="flex items-center gap-3 text-xs text-gray-500">
                  <span>{{ formatDate(flightResult.scheduleDate) }} {{ flightResult.estimatedTime || flightResult.scheduleTime }}</span>
                  <span v-if="flightResult.terminal">터미널: {{ flightResult.terminal }}</span>
                  <span v-if="flightResult.gate">게이트: {{ flightResult.gate }}</span>
                </div>
                <div v-if="flightResult.status" class="mt-2">
                  <span
                    class="text-xs px-2 py-0.5 rounded-full"
                    :class="getStatusClass(flightResult.status)"
                  >
                    {{ flightResult.status }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 검색 결과 없음 -->
        <div v-else-if="searched && !isLoading" class="text-center py-8 text-gray-400">
          검색 결과가 없습니다
        </div>
      </div>
    </template>

    <template #footer>
      <button @click="emit('close')" class="w-full py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 transition-colors">
        닫기
      </button>
    </template>
  </SlideUpModal>
</template>

<script setup>
import { ref, reactive, watch } from 'vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import axios from 'axios'
import dayjs from 'dayjs'

const props = defineProps({
  show: Boolean,
  flightType: {
    type: String,
    default: 'departure'
  }
})

const emit = defineEmits(['close', 'apply'])

const searchForm = reactive({
  flightNumber: '',
  date: dayjs().format('YYYY-MM-DD')
})

const flightResult = ref(null)
const isLoading = ref(false)
const errorMessage = ref('')
const searched = ref(false)

watch(() => props.show, (newVal) => {
  if (newVal) {
    // 모달이 열릴 때 초기화
    searchForm.flightNumber = ''
    searchForm.date = dayjs().format('YYYY-MM-DD')
    flightResult.value = null
    errorMessage.value = ''
    searched.value = false
  }
})

async function searchFlight() {
  if (!searchForm.flightNumber || !searchForm.date) {
    errorMessage.value = '항공편 번호와 날짜를 입력해주세요.'
    return
  }

  isLoading.value = true
  errorMessage.value = ''
  flightResult.value = null
  searched.value = true

  try {
    const response = await axios.get('/api/flight/incheon', {
      params: {
        flightNumber: searchForm.flightNumber,
        date: searchForm.date,
        flightType: props.flightType.toUpperCase()
      }
    })

    flightResult.value = response.data
  } catch (error) {
    console.error('항공편 검색 오류:', error)
    if (error.response?.status === 404) {
      errorMessage.value = '해당 항공편을 찾을 수 없습니다. 항공편 번호와 날짜를 확인해주세요.'
    } else if (error.response?.status === 400) {
      errorMessage.value = error.response.data.message || '입력 정보를 확인해주세요.'
    } else {
      errorMessage.value = '항공편 검색 중 오류가 발생했습니다.'
    }
  } finally {
    isLoading.value = false
  }
}

function selectFlight(flight) {
  emit('apply', { ...flight })
  emit('close')
}

function formatDate(dateStr) {
  if (!dateStr || dateStr.length !== 8) return dateStr
  return `${dateStr.substring(0, 4)}-${dateStr.substring(4, 6)}-${dateStr.substring(6, 8)}`
}

function getStatusClass(status) {
  if (status.includes('출발') || status.includes('도착') || status.includes('탑승')) {
    return 'bg-green-100 text-green-700'
  } else if (status.includes('지연') || status.includes('결항')) {
    return 'bg-red-100 text-red-700'
  } else {
    return 'bg-gray-100 text-gray-700'
  }
}
</script>

<style scoped>
.label {
  @apply block text-sm font-medium text-gray-700 mb-1;
}

.input {
  @apply block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm;
}
</style>
