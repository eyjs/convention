<template>
  <div class="min-h-screen relative bg-gray-50">
    <!-- Decorative Background Elements -->
    <div class="fixed inset-0 z-0 overflow-hidden pointer-events-none">
      <!-- Large gradient blobs -->
      <div class="absolute -top-40 -right-40 w-96 h-96 bg-gradient-to-br from-sky-200/15 to-blue-200/15 rounded-full blur-3xl"></div>
      <div class="absolute top-1/3 -left-32 w-80 h-80 bg-gradient-to-br from-blue-200/12 to-cyan-200/12 rounded-full blur-3xl"></div>
      <div class="absolute bottom-20 right-1/4 w-64 h-64 bg-gradient-to-br from-cyan-200/12 to-sky-200/12 rounded-full blur-3xl"></div>

      <!-- Subtle pattern overlay -->
      <div class="absolute inset-0 opacity-[0.02]" style="background-image: url('data:image/svg+xml,%3Csvg width=&quot;60&quot; height=&quot;60&quot; viewBox=&quot;0 0 60 60&quot; xmlns=&quot;http://www.w3.org/2000/svg&quot;%3E%3Cg fill=&quot;none&quot; fill-rule=&quot;evenodd&quot;%3E%3Cg fill=&quot;%239C92AC&quot; fill-opacity=&quot;1&quot;%3E%3Cpath d=&quot;M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z&quot;/%3E%3C/g%3E%3C/g%3E%3C/svg%3E');"></div>
    </div>

    <div class="relative z-10">
      <MainHeader title="홈" :show-back="false" />

      <div class="max-w-2xl mx-auto px-4 py-4">
      <!-- 환영 메시지 (Hero Section) -->
      <div class="relative overflow-hidden bg-gradient-to-br from-violet-600 via-purple-600 to-fuchsia-600 rounded-2xl shadow-xl p-8 mb-8 text-white">
        <div class="relative z-10">
          <h2 class="text-3xl font-bold mb-2">안녕하세요,<br/>{{ userName }}님! ✈️</h2>
          <p class="text-lg text-white/90">다음 여행을 계획해보세요</p>
        </div>
        <!-- Decorative elements -->
        <div class="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -mr-16 -mt-16"></div>
        <div class="absolute bottom-0 right-8 w-24 h-24 bg-white/10 rounded-full -mb-12"></div>
      </div>

      <!-- 빠른 액션 -->
      <div class="grid grid-cols-2 gap-4 mb-8">
        <button @click="goToCreateTrip" class="relative overflow-hidden bg-gradient-to-br from-cyan-500 to-blue-600 rounded-2xl shadow-lg p-6 hover:shadow-xl transition-all transform hover:scale-105 active:scale-95">
          <div class="flex flex-col items-center gap-3 text-white">
            <div class="w-14 h-14 bg-white/20 backdrop-blur-sm rounded-full flex items-center justify-center">
              <svg class="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 4v16m8-8H4" />
              </svg>
            </div>
            <span class="text-base font-bold">새 여행</span>
          </div>
          <div class="absolute -bottom-2 -right-2 w-20 h-20 bg-white/10 rounded-full"></div>
        </button>

        <button @click="router.push('/')" class="relative overflow-hidden bg-gradient-to-br from-rose-500 to-orange-500 rounded-2xl shadow-lg p-6 hover:shadow-xl transition-all transform hover:scale-105 active:scale-95">
          <div class="flex flex-col items-center gap-3 text-white">
            <div class="w-14 h-14 bg-white/20 backdrop-blur-sm rounded-full flex items-center justify-center">
              <svg class="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
            </div>
            <span class="text-base font-bold">스타투어</span>
          </div>
          <div class="absolute -bottom-2 -right-2 w-20 h-20 bg-white/10 rounded-full"></div>
        </button>
      </div>

      <!-- 내 여행 섹션 -->
      <div class="mb-8">
        <div class="flex justify-between items-center mb-4 px-1">
          <h3 class="text-xl font-bold text-gray-900">내 여행</h3>
          <button v-if="trips.length > 0" @click="router.push('/trips')" class="text-sm font-semibold text-blue-600 hover:text-blue-700 transition-colors">
            더보기
          </button>
        </div>

        <div v-if="tripsLoading" class="text-center py-12 text-gray-500">
          <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600 mx-auto"></div>
        </div>

        <div v-else-if="trips.length === 0" class="bg-white rounded-2xl shadow-md p-8 text-center mx-1">
          <div class="w-20 h-20 mx-auto mb-4 bg-gray-100 rounded-full flex items-center justify-center">
            <svg class="w-10 h-10 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <p class="text-gray-600 mb-2 font-medium">아직 여행 계획이 없습니다</p>
          <p class="text-sm text-gray-400 mb-4">첫 여행을 계획해보세요!</p>
          <button @click="goToCreateTrip" class="inline-flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-cyan-500 to-blue-600 text-white rounded-full font-semibold hover:shadow-lg transition-shadow">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            첫 여행 만들기
          </button>
        </div>

        <!-- 수평 스크롤 카드 -->
        <div v-else class="overflow-x-auto -mx-4 px-4 scrollbar-hide">
          <div class="flex gap-4 pb-2">
            <div
              v-for="trip in trips.slice(0, 10)"
              :key="trip.id"
              @click="goToTripDetail(trip.id)"
              class="flex-shrink-0 w-[280px] bg-white rounded-2xl shadow-md hover:shadow-xl transition-all cursor-pointer overflow-hidden group">
              <!-- 상단 이미지 영역 (인스타그램 스타일) -->
              <div class="relative h-[200px] overflow-hidden">
                <!-- 사용자 업로드 이미지 또는 기본 그라데이션 -->
                <div v-if="trip.coverImageUrl" class="absolute inset-0">
                  <img :src="trip.coverImageUrl" :alt="trip.title" class="w-full h-full object-cover" />
                </div>
                <div v-else class="absolute inset-0 bg-gradient-to-br from-cyan-500 via-teal-500 to-blue-600">
                  <div class="absolute inset-0 flex items-center justify-center">
                    <svg class="w-20 h-20 text-white/25" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                  <!-- Decorative elements -->
                  <div class="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -mr-16 -mt-16"></div>
                  <div class="absolute bottom-0 left-0 w-24 h-24 bg-white/10 rounded-full -ml-12 -mb-12"></div>
                </div>
              </div>

              <!-- 카드 정보 (인스타그램 스타일) -->
              <div class="p-4">
                <h4 class="font-bold text-gray-900 text-lg mb-2 line-clamp-1 group-hover:text-cyan-600 transition-colors">{{ trip.title }}</h4>

                <!-- 날짜 -->
                <div class="flex items-center gap-2 text-sm text-gray-500 mb-2">
                  <svg class="w-4 h-4 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  <span class="truncate">{{ trip.startDate }} ~ {{ trip.endDate }}</span>
                </div>

                <!-- 위치 -->
                <div v-if="trip.destination || trip.city" class="flex items-center gap-2 text-sm text-gray-600">
                  <svg class="w-4 h-4 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  <span class="font-semibold truncate">{{ [trip.destination, trip.city].filter(Boolean).join(', ') }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 내 행사 섹션 -->
      <div class="mb-8">
        <div class="flex justify-between items-center mb-4 px-1">
          <h3 class="text-xl font-bold text-gray-900">진행중인 스타투어</h3>
          <button v-if="conventions.length > 0" @click="router.push('/')" class="text-sm font-semibold text-blue-600 hover:text-blue-700 transition-colors">
            더보기
          </button>
        </div>

        <div v-if="conventionsLoading" class="text-center py-12 text-gray-500">
          <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600 mx-auto"></div>
        </div>

        <div v-else-if="conventions.length === 0" class="bg-white rounded-2xl shadow-md p-8 text-center mx-1">
          <div class="w-20 h-20 mx-auto mb-4 bg-gray-100 rounded-full flex items-center justify-center">
            <svg class="w-10 h-10 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            </svg>
          </div>
          <p class="text-gray-600 font-medium">참여 중인 행사가 없습니다</p>
        </div>

        <!-- 수평 스크롤 카드 -->
        <div v-else class="overflow-x-auto -mx-4 px-4 scrollbar-hide">
          <div class="flex gap-4 pb-2">
            <div
              v-for="convention in conventions.slice(0, 10)"
              :key="convention.id"
              @click="goToConvention(convention)"
              class="flex-shrink-0 w-[280px] bg-white rounded-2xl shadow-md hover:shadow-xl transition-all cursor-pointer overflow-hidden group">
              <!-- 상단 이미지 영역 -->
              <div class="relative h-[200px] bg-gradient-to-br from-rose-500 via-orange-500 to-amber-500 overflow-hidden">
                <div class="absolute inset-0 flex items-center justify-center">
                  <svg class="w-20 h-20 text-white/25" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                  </svg>
                </div>
                <!-- Decorative elements -->
                <div class="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -mr-16 -mt-16"></div>
                <div class="absolute bottom-0 left-0 w-24 h-24 bg-white/10 rounded-full -ml-12 -mb-12"></div>
              </div>

              <!-- 카드 정보 -->
              <div class="p-4">
                <h4 class="font-bold text-gray-900 text-lg mb-2 line-clamp-1 group-hover:text-rose-600 transition-colors">{{ convention.title }}</h4>

                <!-- 날짜 -->
                <div class="flex items-center gap-2 text-sm text-gray-500">
                  <svg class="w-4 h-4 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  <span class="truncate">{{ formatDate(convention.startDate) }} ~ {{ formatDate(convention.endDate) }}</span>
                </div>
              </div>
            </div>
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

<style scoped>
/* 수평 스크롤바 숨기기 */
.scrollbar-hide {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
.scrollbar-hide::-webkit-scrollbar {
  display: none;
}
</style>
