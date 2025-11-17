<template>
  <div class="min-h-screen relative bg-gray-50">
    <!-- Decorative Background Elements -->
    <div class="fixed inset-0 z-0 overflow-hidden pointer-events-none">
      <!-- Large gradient blobs -->
      <div class="absolute -top-32 -left-32 w-80 h-80 bg-gradient-to-br from-sky-200/15 to-blue-200/15 rounded-full blur-3xl"></div>
      <div class="absolute top-1/2 -right-40 w-96 h-96 bg-gradient-to-br from-blue-200/12 to-cyan-200/12 rounded-full blur-3xl"></div>
      <div class="absolute bottom-32 left-1/4 w-72 h-72 bg-gradient-to-br from-cyan-200/12 to-sky-200/12 rounded-full blur-3xl"></div>

      <!-- Subtle wave pattern -->
      <div class="absolute inset-0 opacity-[0.02]" style="background-image: url('data:image/svg+xml,%3Csvg width=&quot;100&quot; height=&quot;20&quot; viewBox=&quot;0 0 100 20&quot; xmlns=&quot;http://www.w3.org/2000/svg&quot;%3E%3Cpath d=&quot;M21.184 20c.357-.13.72-.264 1.088-.402l1.768-.661C33.64 15.347 39.647 14 50 14c10.271 0 15.362 1.222 24.629 4.928.955.383 1.869.74 2.75 1.072h6.225c-2.51-.73-5.139-1.691-8.233-2.928C65.888 13.278 60.562 12 50 12c-10.626 0-16.855 1.397-26.66 5.063l-1.767.662c-2.475.923-4.66 1.674-6.724 2.275h6.335zm0-20C13.258 2.892 8.077 4 0 4V2c5.744 0 9.951-.574 14.85-2h6.334zM77.38 0C85.239 2.966 90.502 4 100 4V2c-6.842 0-11.386-.542-16.396-2h-6.225zM0 14c8.44 0 13.718-1.21 22.272-4.402l1.768-.661C33.64 5.347 39.647 4 50 4c10.271 0 15.362 1.222 24.629 4.928C84.112 12.722 89.438 14 100 14v-2c-10.271 0-15.362-1.222-24.629-4.928C65.888 3.278 60.562 2 50 2 39.374 2 33.145 3.397 23.34 7.063l-1.767.662C13.223 10.84 8.163 12 0 12v2z&quot; fill=&quot;%239C92AC&quot; fill-opacity=&quot;1&quot; fill-rule=&quot;evenodd&quot;/%3E%3C/svg%3E');"></div>
    </div>

    <div class="relative z-10">
      <MainHeader title="내 여행" :show-back="false" />

      <div class="max-w-2xl mx-auto px-4 py-4">
      <!-- 로딩 상태 -->
      <div v-if="loading" class="text-center py-16">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-4 text-gray-600 font-medium">로딩 중...</p>
      </div>

      <!-- 여행 목록 -->
      <template v-else>
        <!-- Empty State -->
        <div v-if="trips.length === 0" class="text-center py-16">
          <div class="w-24 h-24 mx-auto mb-6 bg-primary-100 rounded-full flex items-center justify-center">
            <svg class="w-12 h-12 text-primary-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <h3 class="text-xl font-bold text-gray-900 mb-2">아직 여행이 없습니다</h3>
          <p class="text-gray-500 mb-6">첫 여행을 계획해보세요!</p>
          <button @click="goToCreateTrip" class="inline-flex items-center gap-2 px-6 py-3 bg-primary-500 text-white rounded-full font-semibold hover:shadow-lg transition-shadow">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            새 여행 만들기
          </button>
        </div>

        <!-- 여행 카드 목록 -->
        <div v-else class="space-y-5">
          <div v-for="trip in trips" :key="trip.id" @click="goToTripDetail(trip.id)" class="group bg-white rounded-2xl shadow-md hover:shadow-2xl transition-all cursor-pointer overflow-hidden">
            <!-- 카드 헤더 (이미지 영역) -->
            <div class="relative h-40 overflow-hidden">
              <!-- 사용자 업로드 이미지 또는 기본 그라데이션 -->
              <div v-if="trip.coverImageUrl" class="absolute inset-0">
                <img :src="trip.coverImageUrl" :alt="trip.title" class="w-full h-full object-cover" />
              </div>
              <div v-else class="absolute inset-0 bg-gradient-to-br from-cyan-500 via-teal-500 to-blue-600">
                <div class="absolute inset-0 flex items-center justify-center">
                  <svg class="w-16 h-16 text-white/30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <!-- Decorative elements -->
                <div class="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -mr-16 -mt-16"></div>
                <div class="absolute bottom-0 left-0 w-24 h-24 bg-white/10 rounded-full -ml-12 -mb-12"></div>
              </div>
            </div>

            <!-- 카드 본문 -->
            <div class="p-5">
              <h3 class="text-xl font-bold text-gray-900 mb-2 group-hover:text-primary-600 transition-colors">{{ trip.title }}</h3>
              <p v-if="trip.description" class="text-sm text-gray-600 mb-3 line-clamp-2">{{ trip.description }}</p>

              <!-- 날짜 -->
              <div class="flex items-center gap-2 text-sm text-gray-500 mb-2">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <span class="font-medium">{{ trip.startDate }} ~ {{ trip.endDate }}</span>
              </div>

              <!-- 목적지 -->
              <div v-if="trip.destination || trip.city" class="flex items-center gap-2 text-sm text-gray-600 mb-4">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <span class="font-semibold">{{ [trip.destination, trip.city].filter(Boolean).join(', ') }}</span>
              </div>

              <!-- 태그 -->
              <div class="flex gap-2 flex-wrap">
                <span v-if="trip.flights.length > 0" class="inline-flex items-center gap-1 px-3 py-1.5 bg-blue-50 text-blue-700 rounded-full text-xs font-semibold">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                  </svg>
                  항공 {{ trip.flights.length }}
                </span>
                <span v-if="trip.accommodations.length > 0" class="inline-flex items-center gap-1 px-3 py-1.5 bg-green-50 text-green-700 rounded-full text-xs font-semibold">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                  </svg>
                  숙소 {{ trip.accommodations.length }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </template>
      </div>

      <!-- 새 여행 만들기 FAB -->
      <button @click="goToCreateTrip" class="fixed bottom-8 right-8 bg-primary-500 text-white rounded-full p-5 shadow-2xl hover:shadow-3xl focus:outline-none focus:ring-4 focus:ring-primary-300/50 transition-all transform hover:scale-110 active:scale-95 z-20">
        <PlusIcon class="w-7 h-7" />
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import MainHeader from '@/components/common/MainHeader.vue'
import apiClient from '@/services/api'
import { Plus as PlusIcon } from 'lucide-vue-next'

const router = useRouter()
const loading = ref(true)
const trips = ref([])

async function loadTrips() {
  loading.value = true
  try {
    const response = await apiClient.get('/personal-trips')
    trips.value = response.data
  } catch (error) {
    console.error('여행 목록 로드 실패:', error)
    alert('여행 목록을 불러올 수 없습니다.')
  } finally {
    loading.value = false
  }
}

function goToCreateTrip() {
  router.push('/trips/create')
}

function goToTripDetail(tripId) {
  router.push(`/trips/${tripId}`)
}

onMounted(loadTrips)
</script>
