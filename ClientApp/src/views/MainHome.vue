<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader title="홈" :show-back="false" />

    <div class="max-w-2xl mx-auto px-4 py-6">
      <!-- 환영 메시지 -->
      <div class="bg-gradient-to-r from-blue-600 to-blue-400 rounded-lg shadow p-6 mb-6 text-white">
        <h2 class="text-2xl font-bold">안녕하세요, {{ userName }}님!</h2>
        <p class="mt-2 text-blue-100">오늘도 좋은 하루 되세요.</p>
      </div>

      <!-- 빠른 액션 -->
      <div class="grid grid-cols-2 gap-4 mb-6">
        <button @click="goToCreateTrip" class="bg-white rounded-lg shadow p-4 hover:shadow-md transition-shadow">
          <div class="flex flex-col items-center gap-2">
            <div class="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center">
              <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
            </div>
            <span class="text-sm font-medium text-gray-900">새 여행</span>
          </div>
        </button>

        <button @click="goToConventions" class="bg-white rounded-lg shadow p-4 hover:shadow-md transition-shadow">
          <div class="flex flex-col items-center gap-2">
            <div class="w-12 h-12 bg-green-100 rounded-full flex items-center justify-center">
              <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
            </div>
            <span class="text-sm font-medium text-gray-900">스타투어</span>
          </div>
        </button>
      </div>

      <!-- 내 여행 섹션 -->
      <div class="mb-6">
        <div class="flex justify-between items-center mb-3">
          <h3 class="text-lg font-bold text-gray-900">내 여행</h3>
          <button v-if="trips.length > 3" @click="showAllTrips = !showAllTrips" class="text-sm text-blue-600 hover:text-blue-700">
            {{ showAllTrips ? '접기' : `모두 보기 (${trips.length})` }}
          </button>
        </div>

        <div v-if="tripsLoading" class="text-center py-8 text-gray-500">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
        </div>

        <div v-else-if="trips.length === 0" class="bg-white rounded-lg shadow p-6 text-center text-gray-500">
          <svg class="w-12 h-12 mx-auto text-gray-300 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <p class="text-sm">아직 여행 계획이 없습니다.</p>
          <button @click="goToCreateTrip" class="mt-3 text-sm text-blue-600 hover:text-blue-700">
            첫 여행 만들기
          </button>
        </div>

        <div v-else class="space-y-3">
          <div v-for="trip in displayedTrips" :key="trip.id" @click="goToTripDetail(trip.id)" class="bg-white rounded-lg shadow p-4 hover:shadow-md transition-shadow cursor-pointer">
            <h4 class="font-bold text-gray-900">{{ trip.title }}</h4>
            <div class="mt-2 flex items-center gap-2 text-sm text-gray-500">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <span>{{ trip.startDate }} ~ {{ trip.endDate }}</span>
            </div>
            <div v-if="trip.destination || trip.city" class="mt-1 flex items-center gap-1 text-sm text-gray-600">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
              </svg>
              <span>{{ [trip.destination, trip.city].filter(Boolean).join(', ') }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 내 행사 섹션 -->
      <div class="mb-6">
        <div class="flex justify-between items-center mb-3">
          <h3 class="text-lg font-bold text-gray-900">진행중인 스타투어</h3>
          <button v-if="conventions.length > 3" @click="showAllConventions = !showAllConventions" class="text-sm text-blue-600 hover:text-blue-700">
            {{ showAllConventions ? '접기' : `모두 보기 (${conventions.length})` }}
          </button>
        </div>

        <div v-if="conventionsLoading" class="text-center py-8 text-gray-500">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
        </div>

        <div v-else-if="conventions.length === 0" class="bg-white rounded-lg shadow p-6 text-center text-gray-500">
          <svg class="w-12 h-12 mx-auto text-gray-300 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
          </svg>
          <p class="text-sm">참여 중인 행사가 없습니다.</p>
        </div>

        <div v-else class="space-y-3">
          <div v-for="convention in displayedConventions" :key="convention.id" @click="goToConvention(convention)" class="bg-white rounded-lg shadow p-4 hover:shadow-md transition-shadow cursor-pointer">
            <h4 class="font-bold text-gray-900">{{ convention.title }}</h4>
            <div class="mt-2 flex items-center gap-2 text-sm text-gray-500">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <span>{{ formatDate(convention.startDate) }} ~ {{ formatDate(convention.endDate) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import MainHeader from '@/components/common/MainHeader.vue'
import apiClient from '@/services/api'

const router = useRouter()
const authStore = useAuthStore()
const conventionStore = useConventionStore()

const userName = computed(() => authStore.user?.name || '사용자')

const tripsLoading = ref(true)
const conventionsLoading = ref(true)
const trips = ref([])
const conventions = ref([])
const showAllTrips = ref(false)
const showAllConventions = ref(false)

// 여행 목록 (전체 또는 최근 3개)
const displayedTrips = computed(() => showAllTrips.value ? trips.value : trips.value.slice(0, 3))
// 행사 목록 (전체 또는 최근 3개)
const displayedConventions = computed(() => showAllConventions.value ? conventions.value : conventions.value.slice(0, 3))

// 여행 목록 로드
async function loadTrips() {
  tripsLoading.value = true
  try {
    const response = await apiClient.get('/personal-trips')
    trips.value = response.data
  } catch (error) {
    console.error('여행 목록 로드 실패:', error)
    trips.value = []
  } finally {
    tripsLoading.value = false
  }
}

// 행사 목록 로드
async function loadConventions() {
  conventionsLoading.value = true
  try {
    const response = await apiClient.get('/users/conventions')
    conventions.value = response.data
  } catch (error) {
    console.error('행사 목록 로드 실패:', error)
    conventions.value = []
  } finally {
    conventionsLoading.value = false
  }
}

// 네비게이션
function goToCreateTrip() {
  router.push('/trips/create')
}

function goToTripDetail(tripId) {
  router.push(`/trips/${tripId}`)
}

async function goToConvention(convention) {
  await conventionStore.selectConvention(convention.id)
  router.push('/')
}

function goToConventions() {
  showAllConventions.value = true
  // 회사 행사 섹션으로 스크롤
  setTimeout(() => {
    const conventionsSection = document.querySelector('.mb-6:last-of-type')
    if (conventionsSection) {
      conventionsSection.scrollIntoView({ behavior: 'smooth', block: 'start' })
    }
  }, 100)
}

// 날짜 포맷
function formatDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('ko-KR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  })
}

onMounted(() => {
  loadTrips()
  loadConventions()
})
</script>
